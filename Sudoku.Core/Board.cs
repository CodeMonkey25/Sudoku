using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku
{
    public sealed class Board : IDisposable
    {
        public Cell[] Cells { get; } = Enumerable.Range(0, 81).Select(static i => new Cell(i)).ToArray();
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

            for (int i = 0; i < Cells.Length; i++)
            {
                int row = i / 9;
                int currentIndex = currentRowIndexes[row]++;
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
            foreach (Cell[] cell in Rows)
            {
                BindCells(cell);
            }

            foreach (Cell[] cell in Columns)
            {
                BindCells(cell);
            }

            foreach (Cell[] cell in Grids)
            {
                BindCells(cell);
            }
        }

        private static void BindCells(Cell[] cells)
        {
            foreach (Cell cell in cells)
            {
                cell.BindTo(cells);
            }
        }

        public string GetOriginalPuzzle()
        {
            return string.Join(",", Cells.Select(static cell => cell.Value == 0 ? string.Empty : cell.Value.ToString()));
        }

        public int[] GetSolution()
        {
            return Cells.Select(static cell => cell.Value).ToArray();
        }

        public HashSet<int>[] GetState()
        {
            return Cells.Select(static cell => cell.GetState()).ToArray();
        }
        
        public void RestoreState(HashSet<int>[] state)
        {
            foreach (Cell cell in Cells)
            {
                cell.Reset();
            }

            for (int i = 0; i < state.Length; i++)
            {
                HashSet<int> cellState = state[i];
                Cells[i].SetState(cellState);
            }
        }

        public static int[] ParsePuzzle(string puzzle)
        {
            int[] loadedPuzzle = [];

            if (!string.IsNullOrEmpty(puzzle))
            {
                if (puzzle.Contains(' '))
                    loadedPuzzle = ParsePuzzleWithSpaces(puzzle);

                if (puzzle.Contains(','))
                    loadedPuzzle = ParsePuzzleWithCommas(puzzle);
            }

            if (loadedPuzzle.Length != 81)
                throw new Exception("Puzzle is malformed: cell count is not 81");

            return loadedPuzzle;
        }

        private static int[] ParsePuzzleWithCommas(string puzzle)
        {
            // comma seperated format (4,,,,9,,,8 ...)

            int[] loadedPuzzle = new int[81];
            int i = 0;
            foreach (char c in puzzle)
            {
                if (i >= loadedPuzzle.Length) throw new Exception("Puzzle is malformed: cell count is not 81");

                switch (c)
                {
                    case ',':
                        i++;
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        loadedPuzzle[i] = c - '0';
                        break;
                }
            }

            if (i != (loadedPuzzle.Length - 1)) throw new Exception("Puzzle is malformed: cell count is not 81");

            return loadedPuzzle;
        }

        private static int[] ParsePuzzleWithSpaces(string puzzle)
        {
            // alternate format (530 070 000 ...)

            int j = 0;
            int[] loadedPuzzle = new int[81];
            foreach (char c in puzzle.Where(char.IsDigit))
            {
                if (j >= loadedPuzzle.Length) throw new Exception("Puzzle is malformed: cell count is not 81");
                loadedPuzzle[j++] = c - '0';
            }

            if (j != loadedPuzzle.Length) throw new Exception("Puzzle is malformed: cell count is not 81");
            return loadedPuzzle;
        }
            
        public void LoadPuzzle(int[] puzzle)
        {
            if (puzzle.Length != Cells.Length) throw new Exception("Puzzle length does not match board length!");

            for (int i = 0; i < puzzle.Length; i++)
            {
                Cells[i].Reset();
            }

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

            foreach (Cell cell in Cells)
            {
                if (cell.Index <= 9) sb.Append('0');
                sb.Append(cell.Index);
                sb.Append(" => ");
                sb.AppendJoin(", ", cell.Candidates.OrderBy(static i => i));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public bool CheckForLoneCandidates(Action<string> log)
        {
            return Enumerable.Range(1, 9)
                // .AsParallel() // bad idea, race condition on candidates
                .Select(i => CheckForLoneCandidates(log, i))
                .Aggregate(false, (acc, val) => acc || val);
        }

        private bool CheckForLoneCandidates(Action<string> log, int value)
        {
            // check rows
            bool boardChanged = CheckForLoneCandidates(log, Rows, value);

            // check columns
            if (CheckForLoneCandidates(log, Columns, value)) boardChanged = true;

            // check grids
            if (CheckForLoneCandidates(log, Grids, value)) boardChanged = true;

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Action<string> log, Cell[][] cellGrouping, int value)
        {
            bool boardChanged = false;
            foreach (Cell[] cell in cellGrouping)
            {
                if (CheckForLoneCandidates(log, cell, value)) boardChanged = true;
            }

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Action<string> log, Cell[] cells, int value)
        {
            Cell? loneCandidate = null;
            foreach (Cell cell in cells)
            {
                if (cell.Value == value) return false; // already solved with this value
                if (cell.IsSolved) continue; // already solved with a different value
                if (!cell.Candidates.Contains(value)) continue; // can't be this value
                if (loneCandidate != null) return false; // already have a candidate, so not a lone candidate

                loneCandidate = cell;
            }

            if (loneCandidate == null) return false;
            log($"Lone Candidate: Cell #{loneCandidate.Index} solved to {value}");
            loneCandidate.Solve(value);
            return true;
        }

        public bool CheckForDeadlockedCells(Action<string> log)
        {
            // check rows
            bool boardChanged = CheckForDeadlockedCells(log, Rows);

            // check columns
            if (CheckForDeadlockedCells(log, Columns)) boardChanged = true;

            // check grids
            if (CheckForDeadlockedCells(log, Grids)) boardChanged = true;

            return boardChanged;
        }

        private static bool CheckForDeadlockedCells(Action<string> log, Cell[][] cellGrouping)
        {
            return cellGrouping
                // .AsParallel() // bad idea, race condition on candidates
                .Select(cells => CheckForDeadlockedCells(log, cells))
                .Aggregate(false, (acc, val) => acc || val);
        }

        private static bool CheckForDeadlockedCells(Action<string> log, Cell[] cells)
        {
            bool boardChanged = false;

            IEqualityComparer<ISet<int>> comparer = new SetEqualityComparer<int>();
            
            var groups = cells.GroupBy(c => c.Candidates, comparer)
                .Where(static g => g.Count() > 1)
                .Where(static g => g.Count() < 5) // what would be best here? anything under 9?
                .Where(static g => g.Key.Count == g.Count())
                .ToArray();

            foreach (IGrouping<ISet<int>, Cell> grouping in groups)
            {
                ISet<int> candidates = grouping.Key;
                HashSet<Cell> deadlockedCells = grouping.ToHashSet();

                string cellsText = string.Join(", ", deadlockedCells.Select(static c => c.Index));
                string candidatesText = string.Join(", ", candidates);
                log($"Found deadlock: Cells #({cellsText}) locks values {candidatesText}");

                foreach (Cell cell in cells)
                {
                    if (deadlockedCells.Contains(cell)) continue;
                    if (cell.RemoveCandidates(candidates)) boardChanged = true;
                }
            }

            return boardChanged;
        }

        public bool IsUnsolved()
        {
            return Cells.Any(cell => !cell.IsSolved);
        }

        public bool IsSolved()
        {
            return Cells.All(cell => cell.IsSolved);
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
            return cellGrouping.All(IsSolutionValid);
        }

        private static bool IsSolutionValid(Cell[] cells)
        {
            HashSet<int> values = [];
            foreach (Cell cell in cells)
            {
                if (!cell.IsSolved) return false;
                values.Add(cell.Value);
            }

            return values.Count == 9;
        }

        public Cell GetCellWithLeastAmountOfCandidates()
        {
            return Cells
                .Where(static c => !c.IsSolved)
                .MinBy(static c => c.Candidates.Count)!;
        }
    }
}