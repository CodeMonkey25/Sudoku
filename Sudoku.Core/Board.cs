using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Sudoku
{
    public sealed class Board : IDisposable
    {
        public Cell[] Cells { get; } = Enumerable.Range(0, 81).Select(static i => new Cell(i)).ToArray();
        private Cell[][] Rows { get; } = Enumerable.Range(0, 9).Select(static _ => new Cell[9]).ToArray();
        private Cell[][] Columns { get; } = Enumerable.Range(0, 9).Select(static _ => new Cell[9]).ToArray();
        private Cell[][] Grids { get; } = Enumerable.Range(0, 9).Select(static _ => new Cell[9]).ToArray();

        private int _unsolvedCount;

        public Board()
        {
            InitializeGroupings();
            BindCells();

            _unsolvedCount = Cells.Length;
            foreach (Cell cell in Cells)
            {
                cell.IsSolvedChanged += OnCellIsSolvedChanged;
            }
        }

        private void OnCellIsSolvedChanged(Cell cell, bool isSolved)
        {
            _unsolvedCount += isSolved ? -1 : 1;
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

        public string GetPuzzle() => string.Join(",", Cells.Select(static cell => cell.Value == 0 ? string.Empty : cell.Value.ToString()));

        public int[] GetSolution() => Cells.Select(static cell => cell.Value).ToArray();

        public void GetState(out BoardState state)
        {
            state = default;
            for (int i = 0; i < Cells.Length; i++)
            {
                state[i] = Cells[i].GetState();
            }
        }

        public void RestoreState(in BoardState state)
        {
            for (int i = 0; i < Cells.Length; i++)
            {
                Cells[i].SetState(state[i]);
            }
        }

        public static int[] ParsePuzzle(string puzzle)
        {
            int[] loadedPuzzle = [];

            if (!string.IsNullOrEmpty(puzzle))
            {
                if (puzzle.Contains(','))
                {
                    loadedPuzzle = ParsePuzzleWithCommas(puzzle);
                }
                else if (puzzle.Contains(' '))
                {
                    loadedPuzzle = ParsePuzzleWithSpaces(puzzle);
                }
                else
                {
                    // digits only format (530070000...), tolerating surrounding whitespace such as a trailing new line
                    string trimmed = puzzle.Trim();
                    if (trimmed.Length == 81 && trimmed.All(char.IsDigit))
                    {
                        loadedPuzzle = trimmed.Select(c => c - '0').ToArray();
                    }
                }
            }

            if (loadedPuzzle.Length != 81)
                throw new Exception("Puzzle is malformed: cell count is not 81");

            return loadedPuzzle;
        }

        private static int[] ParsePuzzleWithCommas(string puzzle)
        {
            // comma separated format (4,,,,9,,,8 ...)

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
            
        public void LoadPuzzle(int[] puzzle, out string error)
        {
            error = string.Empty;
            if (puzzle.Length != Cells.Length) 
            {
                error = "Puzzle length does not match board length!";
                return;
            }

            for (int i = 0; i < puzzle.Length; i++)
            {
                Cells[i].Reset();
            }

            for (int i = 0; i < puzzle.Length; i++)
            {
                if (puzzle[i] == 0) continue;
                Cells[i].Solve(puzzle[i], out error, true);
                if (!string.IsNullOrEmpty(error)) return;
            }
        }

        public string CandidatesListing()
        {
            StringBuilder sb = new();

            Span<int> buffer = stackalloc int[9];
            foreach (Cell cell in Cells)
            {
                if (cell.Index <= 9) sb.Append('0');
                sb.Append(cell.Index);
                sb.Append(" => ");
                bool addComma = false;
                foreach (int candidate in cell.GetCandidates(buffer))
                {
                    if (addComma) sb.Append(", ");
                    sb.Append(candidate);
                    addComma = true;
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public bool CheckForLoneCandidates(Action<string> log, out string error)
        {
            error = string.Empty;
            bool boardChanged = false;
            for (int i = 1; i <= 9; i++)
            {
                if (CheckForLoneCandidates(log, i, out error)) boardChanged = true;
                if (!string.IsNullOrEmpty(error)) return false;
            }
            return boardChanged;
        }

        private bool CheckForLoneCandidates(Action<string> log, int value, out string error)
        {
            error = string.Empty;
            
            // check rows
            bool boardChanged = CheckForLoneCandidates(log, Rows, value, out error);
            if (!string.IsNullOrEmpty(error)) return false;

            // check columns
            if (CheckForLoneCandidates(log, Columns, value, out error)) boardChanged = true;
            if (!string.IsNullOrEmpty(error)) return false;

            // check grids
            if (CheckForLoneCandidates(log, Grids, value, out error)) boardChanged = true;
            if (!string.IsNullOrEmpty(error)) return false;

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Action<string> log, Cell[][] cellGrouping, int value, out string error)
        {
            error = string.Empty;
            bool boardChanged = false;
            foreach (Cell[] cell in cellGrouping)
            {
                if (CheckForLoneCandidates(log, cell, value, out error)) boardChanged = true;
                if (!string.IsNullOrEmpty(error)) return false;
            }

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Action<string> log, Cell[] cells, int value, out string error)
        {
            error = string.Empty;
            Cell? loneCandidate = null;
            foreach (Cell cell in cells)
            {
                if (cell.Value == value) return false; // already solved with this value
                if (cell.IsSolved) continue; // already solved with a different value
                if (!cell.HasCandidate(value)) continue; // can't be this value
                if (loneCandidate != null) return false; // already have a candidate, so not a lone candidate

                loneCandidate = cell;
            }

            if (loneCandidate == null) return false;
            log($"Lone Candidate: Cell #{loneCandidate.Index} solved to {value}");
            loneCandidate.Solve(value, out error);
            return string.IsNullOrEmpty(error);
        }

        public bool CheckForDeadlockedCells(Action<string> log, out string error)
        {
            // check rows
            bool boardChanged = CheckForDeadlockedCells(log, Rows, out error);
            if (!string.IsNullOrEmpty(error)) return false;

            // check columns
            if (CheckForDeadlockedCells(log, Columns, out error)) boardChanged = true;
            if (!string.IsNullOrEmpty(error)) return false;

            // check grids
            if (CheckForDeadlockedCells(log, Grids, out error)) boardChanged = true;
            if (!string.IsNullOrEmpty(error)) return false;

            return boardChanged;
        }

        private static bool CheckForDeadlockedCells(Action<string> log, Cell[][] cellGrouping, out string error)
        {
            error = string.Empty;
            bool boardChanged = false;
            foreach (Cell[] cells in cellGrouping)
            {
                if (CheckForDeadlockedCells(log, cells, out error)) boardChanged = true;
                if (!string.IsNullOrEmpty(error)) return false;
            }
            return boardChanged;
        }

        private static bool CheckForDeadlockedCells(Action<string> log, Cell[] cells, out string error)
        {
            error = string.Empty;
            bool boardChanged = false;
            Span<int> buffer = stackalloc int[9];

            for (int i = 0; i < cells.Length; i++)
            {
                Cell first = cells[i];
                if (first.IsSolved) continue;

                uint mask = first.CandidatesMask;
                int size = BitOperations.PopCount(mask);
                if (size <= 1 || size >= 9) continue; // what would be best here? anything under 9?

                // build membership as a bitset of positions; skip if an earlier cell already owns this mask
                int members = 1 << i;
                bool duplicate = false;
                for (int j = 0; j < cells.Length; j++)
                {
                    if (j == i || cells[j].IsSolved || cells[j].CandidatesMask != mask) continue;
                    if (j < i)
                    {
                        duplicate = true;
                        break;
                    }
                    members |= 1 << j;
                }
                if (duplicate || BitOperations.PopCount((uint)members) != size) continue;

                ReadOnlySpan<int> candidates = Cell.GetCandidatesFromMask(mask, buffer);
                LogDeadlock(log, cells, members, candidates);

                // membership was captured before removal, since removals can cascade into other cells
                for (int k = 0; k < cells.Length; k++)
                {
                    if ((members & (1 << k)) != 0) continue;
                    if (cells[k].RemoveCandidates(candidates, out error)) boardChanged = true;
                    if (!string.IsNullOrEmpty(error)) return false;
                }
            }

            return boardChanged;
        }

        private static void LogDeadlock(Action<string> log, Cell[] cells, int members, ReadOnlySpan<int> candidates)
        {
            StringBuilder candidatesText = new();
            foreach (int candidate in candidates)
            {
                if (candidatesText.Length > 0) candidatesText.Append(", ");
                candidatesText.Append(candidate);
            }
            StringBuilder cellsText = new();
            for (int k = 0; k < cells.Length; k++)
            {
                if ((members & (1 << k)) == 0) continue;
                if (cellsText.Length > 0) cellsText.Append(", ");
                cellsText.Append(cells[k].Index);
            }
            log($"Found deadlock: Cells #({cellsText}) locks values {candidatesText}");
        }

        public bool IsUnsolved()
        {
            return _unsolvedCount > 0;
        }

        public bool IsSolved()
        {
            return _unsolvedCount == 0;
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
            Cell? best = null;
            int bestCount = int.MaxValue;
            foreach (Cell cell in Cells)
            {
                if (cell.IsSolved) continue;
                int count = cell.GetCandidateCount();
                if (count < bestCount)
                {
                    best = cell;
                    bestCount = count;
                    if (count == 2) break; // can't do better than 2 for an unsolved cell
                }
            }
            return best!;
        }
    }
}