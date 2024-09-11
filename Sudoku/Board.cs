using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sudoku
{
    public class Board : IDisposable
    {
        private Cell[] Cells { get; } = Enumerable.Range(0, 81).Select(i => new Cell(i)).ToArray();
        private Cell[][] Rows { get; } = Enumerable.Range(0, 9).Select(_ => new Cell[9]).ToArray();
        private Cell[][] Columns { get; } = Enumerable.Range(0, 9).Select(_ => new Cell[9]).ToArray();
        private Cell[][] Grids { get; } = Enumerable.Range(0, 9).Select(_ => new Cell[9]).ToArray();

        public bool IsUnsolved => Cells.Any(cell => !cell.IsSolved);
        
        public Board()
        {
            InitializeGroupings();
            BindCells();
        }

        public void Dispose()
        {
            foreach (Cell cell in Cells)
            {
                cell.Dispose();
            }
        }

        private void InitializeGroupings()
        {
            int[] currentRowIndexes = new int[9];
            int[] currentColumnIndexes = new int[9];
            int[] currentGridIndexes = new int[9];

            for (int i = 0; i < Cells.Length; i++)
            {
                int currentIndex;

                int row = i / 9;
                currentIndex = currentRowIndexes[row]++;
                Rows[row][currentIndex] = Cells[i];

                int col = i % 9;
                currentIndex = currentColumnIndexes[col]++;
                Columns[col][currentIndex] = Cells[i];

                int grid = (row / 3) * 3 + (col / 3);
                currentIndex = currentGridIndexes[grid]++;
                Grids[grid][currentIndex] = Cells[i];
            }
        }

        private void BindCells()
        {
            foreach (Cell[] cells in Rows)
            {
                BindCells(cells);
            }

            foreach (Cell[] cells in Columns)
            {
                BindCells(cells);
            }

            foreach (Cell[] cells in Grids)
            {
                BindCells(cells);
            }
        }

        private void BindCells(Cell[] cells)
        {
            foreach (Cell cell in cells)
            {
                cell.BindTo(cells);
            }
        }

        public int[] GetSolution()
        {
            return Cells.Select(cell => cell.Value).ToArray();
        }

        public void LoadPuzzle(int[] puzzle)
        {
            if (puzzle.Length != Cells.Length) throw new Exception("Puzzle length does not match board length!");

            for (int i = 0; i < puzzle.Length; i++)
            {
                if (puzzle[i] != 0) Cells[i].Solve(puzzle[i]);
            }
        }

        public string CandidatesListing()
        {
            StringBuilder sb = new();

            foreach (Cell cell in Cells)
            {
                sb.Append(cell.Index.ToString("00"));
                sb.Append(" => ");
                sb.AppendLine(string.Join(", ", cell.GetCandidates().OrderBy(i => i)));
            }

            return sb.ToString();
        }

        // public bool CheckForSolvedCells()
        // {
        //     bool cellChanged = false;
        //     foreach (Cell cell in Cells)
        //     {
        //         if (cell.AttemptToSolve())
        //         {
        //             cellChanged = true;
        //         }
        //     }
        //
        //     return cellChanged;
        // }


        public bool CheckForLoneCandidates()
        {
            bool boardChanged = false;
            foreach (int value in Enumerable.Range(1, 9))
            {
                // check rows
                if (CheckForLoneCandidates(Rows, value)) boardChanged = true;

                // check columns
                if (CheckForLoneCandidates(Columns, value)) boardChanged = true;

                // check grids
                if (CheckForLoneCandidates(Grids, value)) boardChanged = true;
            }

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Cell[][] cellGrouping, int value)
        {
            bool boardChanged = false;
            foreach (Cell[] cells in cellGrouping)
            {
                if (CheckForLoneCandidates(cells, value)) boardChanged = true;
            }

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Cell[] cells, int value)
        {
            Cell? loneCandidate = null;
            foreach (Cell cell in cells)
            {
                if (cell.Value == value) return false; // already solved with this value
                if (cell.IsSolved) continue; // already solved with a different value
                if (!cell.GetCandidates().Contains(value)) continue; // can't be this value
                if (loneCandidate != null) return false; // already have a candidate, so not a lone candidate

                loneCandidate = cell;
            }

            if (loneCandidate == null) return false;
            Debug.WriteLine($"Lone Candidate: Cell #{loneCandidate.Index} solved to {value}");
            loneCandidate.Solve(value);
            return true;
        }

        public bool CheckForDeadlockedCells()
        {
            // check rows
            bool boardChanged = CheckForDeadlockedCells(Rows);

            // check columns
            if (CheckForDeadlockedCells(Columns)) boardChanged = true;

            // check grids
            if (CheckForDeadlockedCells(Grids)) boardChanged = true;

            return boardChanged;
        }

        private static bool CheckForDeadlockedCells(Cell[][] cellGrouping)
        {
            bool boardChanged = false;
            foreach (Cell[] cells in cellGrouping)
            {
                if (CheckForDeadlockedCells(cells)) boardChanged = true;
            }

            return boardChanged;
        }

        private static bool CheckForDeadlockedCells(Cell[] cells)
        {
            //   string.Join(" => ", cells.Select(c=> "(" + string.Join(", ", c.GetCandidates()) + ")"))

            bool boardChanged = false;

            IEqualityComparer<int[]> comparer = new ArrayEqualityComparer<int>();
            
            var groups = cells.GroupBy(c => c.GetCandidates(), comparer)
                .Where(g => g.Count() > 1)
                .Where(g => g.Count() < 5) // what would be best here? anything under 9?
                .Where(g => g.Key.Count() == g.Count())
                .ToArray();

            foreach (IGrouping<IEnumerable<int>, Cell> group in groups)
            {
                HashSet<int> candidates = group.Key.ToHashSet();
                HashSet<Cell> deadlockedCells = group.ToHashSet();

                string cellsText = string.Join(", ", deadlockedCells.Select(c => c.Index));
                string candidatesText = string.Join(", ", candidates);
                Debug.WriteLine($"Found deadlock: Cells #({cellsText}) locks values {candidatesText}");

                foreach (Cell cell in cells)
                {
                    if (deadlockedCells.Contains(cell)) continue;
                    if (cell.RemoveCandidates(candidates)) boardChanged = true;
                }
            }

            return boardChanged;
        }

        public bool IsSolutionValid()
        {
            // check rows
            if (!IsSolutionValid(Rows)) return false;

            // check columns
            if (!IsSolutionValid(Columns)) return false;

            // check grids
            if (!IsSolutionValid(Grids)) return false;

            return true;
        }

        private static bool IsSolutionValid(Cell[][] cellGrouping)
        {
            foreach (Cell[] cells in cellGrouping)
            {
                if (!IsSolutionValid(cells)) return false;
            }

            return true;
        }

        private static bool IsSolutionValid(Cell[] cells)
        {
            return cells.Where(cell => cell.IsSolved).Select(cell => cell.Value).Distinct().Count() == 9;
        }
    }
}