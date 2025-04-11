using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sudoku
{
    internal sealed class Board : IDisposable
    {
        private Cell[] Cells { get; } = Enumerable.Range(0, 81).Select(static i => new Cell(i)).ToArray();
        private Cell[][] Rows { get; } = Enumerable.Range(0, 9).Select(static _ => new Cell[9]).ToArray();
        private Cell[][] Columns { get; } = Enumerable.Range(0, 9).Select(static _ => new Cell[9]).ToArray();
        private Cell[][] Grids { get; } = Enumerable.Range(0, 9).Select(static _ => new Cell[9]).ToArray();

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
            Span<int> currentRowIndexes = stackalloc int[9];
            Span<int> currentColumnIndexes = stackalloc int[9];
            Span<int> currentGridIndexes = stackalloc int[9];

            int currentIndex;
            for (int i = 0; i < Cells.Length; i++)
            {
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
            for (var i = 0; i < Rows.Length; i++)
            {
                BindCells(Rows[i]);
            }

            for (var i = 0; i < Columns.Length; i++)
            {
                BindCells(Columns[i]);
            }

            for (var i = 0; i < Grids.Length; i++)
            {
                BindCells(Grids[i]);
            }
        }

        private static void BindCells(Cell[] cells)
        {
            for (var i = 0; i < cells.Length; i++)
            {
                cells[i].BindTo(cells);
            }
        }

        public int[] GetSolution()
        {
            return Cells.Select(static cell => cell.Value).ToArray();
        }

        public void LoadPuzzle(int[] puzzle)
        {
            if (puzzle.Length != Cells.Length) throw new Exception("Puzzle length does not match board length!");

            for (int i = 0; i < puzzle.Length; i++)
            {
                if (puzzle[i] == 0) continue;
                Cells[i].Solve(puzzle[i]);
                Cells[i].IsGiven = true;
            }
        }

        public string CandidatesListing()
        {
            StringBuilder sb = new();

            for (var i = 0; i < Cells.Length; i++)
            {
                var cell = Cells[i];
                if (cell.Index <= 9) sb.Append('0');
                sb.Append(cell.Index);
                sb.Append(" => ");
                sb.AppendJoin(", ", cell.Candidates.OrderBy(static i => i));
                sb.AppendLine();
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
            return Enumerable.Range(1, 9)
                // .AsParallel() // bad idea, race condition on candidates
                .Select(CheckForLoneCandidates)
                .Aggregate(false, (acc, val) => acc || val);
        }

        private bool CheckForLoneCandidates(int value)
        {
            // check rows
            bool boardChanged = CheckForLoneCandidates(Rows, value);

            // check columns
            if (CheckForLoneCandidates(Columns, value)) boardChanged = true;

            // check grids
            if (CheckForLoneCandidates(Grids, value)) boardChanged = true;

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Cell[][] cellGrouping, int value)
        {
            bool boardChanged = false;
            for (var i = 0; i < cellGrouping.Length; i++)
            {
                if (CheckForLoneCandidates(cellGrouping[i], value)) boardChanged = true;
            }

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Cell[] cells, int value)
        {
            Cell? loneCandidate = null;
            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i].Value == value) return false; // already solved with this value
                if (cells[i].IsSolved) continue; // already solved with a different value
                if (!cells[i].Candidates.Contains(value)) continue; // can't be this value
                if (loneCandidate != null) return false; // already have a candidate, so not a lone candidate

                loneCandidate = cells[i];
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
            return cellGrouping
                // .AsParallel() // bad idea, race condition on candidates
                .Select(CheckForDeadlockedCells)
                .Aggregate(false, (acc, val) => acc || val);
        }

        private static bool CheckForDeadlockedCells(Cell[] cells)
        {
            bool boardChanged = false;

            IEqualityComparer<ISet<int>> comparer = new SetEqualityComparer<int>();
            
            var groups = cells.GroupBy(c => c.Candidates, comparer)
                .Where(static g => g.Count() > 1)
                .Where(static g => g.Count() < 5) // what would be best here? anything under 9?
                .Where(static g => g.Key.Count == g.Count())
                .ToArray();

            for (var i = 0; i < groups.Length; i++)
            {
                ISet<int> candidates = groups[i].Key;
                HashSet<Cell> deadlockedCells = groups[i].ToHashSet();

                string cellsText = string.Join(", ", deadlockedCells.Select(static c => c.Index));
                string candidatesText = string.Join(", ", candidates);
                Debug.WriteLine($"Found deadlock: Cells #({cellsText}) locks values {candidatesText}");

                for (var j = 0; j < cells.Length; j++)
                {
                    if (deadlockedCells.Contains(cells[j])) continue;
                    if (cells[j].RemoveCandidates(candidates)) boardChanged = true;
                }
            }

            return boardChanged;
        }

        public bool IsUnsolved()
        {
            for (int i = 0; i < Cells.Length; i++)
            {
                if (!Cells[i].IsSolved) 
                    return true;
            }
            return false;
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
            for (var i = 0; i < cellGrouping.Length; i++)
            {
                if (!IsSolutionValid(cellGrouping[i])) return false;
            }

            return true;
        }

        private static bool IsSolutionValid(Cell[] cells)
        {
            HashSet<int> values = new();
            for (var i = 0; i < cells.Length; i++)
            {
                if (!cells[i].IsSolved) return false;
                values.Add(cells[i].Value);
            }

            return values.Count == 9;
        }
    }
}