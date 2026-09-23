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

        public string GetOriginalPuzzle()
        {
            return string.Join(",", Cells.Select(static cell => cell.Value == 0 ? string.Empty : cell.Value.ToString()));
        }

        public int[] GetSolution()
        {
            return Cells.Select(static cell => cell.Value).ToArray();
        }

        public BoardState GetState()
        {
            return new BoardState()
            {
                CellStates = Cells.Select(static cell => cell.GetState()).ToArray(),
            };
        }
        
        public void RestoreState(BoardState state)
        {
            foreach (Cell cell in Cells)
            {
                cell.Reset();
            }

            for (int i = 0; i < state.CellStates.Length; i++)
            {
                CellState cellState = state.CellStates[i];
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
            
        public void LoadPuzzle(int[] puzzle, out bool error)
        {
            error = false;
            if (puzzle.Length != Cells.Length) 
            {
                // throw new Exception("Puzzle length does not match board length!");
                error = true;
                return;
            }

            for (int i = 0; i < puzzle.Length; i++)
            {
                Cells[i].Reset();
            }

            for (int i = 0; i < puzzle.Length; i++)
            {
                if (puzzle[i] == 0) continue;
                Cells[i].Solve(puzzle[i], out error);
                if (error) return;
                Cells[i].IsGiven = true;
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

        public bool CheckForLoneCandidates(Action<string> log, out bool error)
        {
            error = false;
            bool boardChanged = false;
            for (int i = 1; i <= 9; i++)
            {
                if (CheckForLoneCandidates(log, i, out error)) boardChanged = true;
                if (error) return false;
            }
            return boardChanged;
        }

        private bool CheckForLoneCandidates(Action<string> log, int value, out bool error)
        {
            error = false;
            
            // check rows
            bool boardChanged = CheckForLoneCandidates(log, Rows, value, out error);
            if (error) return false;

            // check columns
            if (CheckForLoneCandidates(log, Columns, value, out error)) boardChanged = true;
            if (error) return false;

            // check grids
            if (CheckForLoneCandidates(log, Grids, value, out error)) boardChanged = true;
            if (error) return false;

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Action<string> log, Cell[][] cellGrouping, int value, out bool error)
        {
            error = false;
            bool boardChanged = false;
            foreach (Cell[] cell in cellGrouping)
            {
                if (CheckForLoneCandidates(log, cell, value, out error)) boardChanged = true;
                if (error) return false;
            }

            return boardChanged;
        }

        private static bool CheckForLoneCandidates(Action<string> log, Cell[] cells, int value, out bool error)
        {
            error = false;
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
            return !error;
        }

        public bool CheckForDeadlockedCells(Action<string> log, out bool error)
        {
            Dictionary<int, List<Cell>> maskMap = new();
            
            // check rows
            bool boardChanged = CheckForDeadlockedCells(log, Rows, maskMap, out error);
            if (error) return false;

            // check columns
            if (CheckForDeadlockedCells(log, Columns, maskMap, out error)) boardChanged = true;
            if (error) return false;

            // check grids
            if (CheckForDeadlockedCells(log, Grids, maskMap, out error)) boardChanged = true;
            if (error) return false;

            return boardChanged;
        }

        private static bool CheckForDeadlockedCells(Action<string> log, Cell[][] cellGrouping, Dictionary<int, List<Cell>> maskMap, out bool error)
        {
            error = false;
            bool boardChanged = false;
            foreach (Cell[] cells in cellGrouping)
            {
                if (CheckForDeadlockedCells(log, cells, maskMap, out error)) boardChanged = true;
                if (error) return false;
            }
            return boardChanged;
        }

        private static bool CheckForDeadlockedCells(Action<string> log, Cell[] cells, Dictionary<int, List<Cell>> maskMap, out bool error)
        {
            error = false;
            bool boardChanged = false;

            foreach (List<Cell> list in maskMap.Values) list.Clear();
            
            foreach (Cell cell in cells)
            {
                if (cell.IsSolved) continue;

                int mask = cell.CandidateMask;
                if (maskMap.TryGetValue(mask, out List<Cell>? group))
                {
                    group.Add(cell);
                }
                else
                {
                    maskMap[mask] = new List<Cell>(9) { cell, };
                }
            }
            
            Span<int> buffer = stackalloc int[9];
            StringBuilder cellsText = new();
            StringBuilder candidatesText = new();
            foreach ((int mask, List<Cell> group) in maskMap)
            {
                if (group.Count <= 1) continue;
                if (group.Count >= 9) continue; // what would be best here? anything under 9?
                if (BitOperations.PopCount((uint)mask) != group.Count) continue;

                ReadOnlySpan<int> candidates = Cell.GetCandidatesFromMask(mask, buffer);
                
                candidatesText.Clear();
                foreach (int candidate in candidates)
                {
                    if (candidatesText.Length > 0) candidatesText.Append(", ");
                    candidatesText.Append(candidate);
                }
                cellsText.Clear();
                foreach (Cell cell in group)
                {
                    if (cellsText.Length > 0) cellsText.Append(", ");
                    cellsText.Append(cell.Index);
                }
                log($"Found deadlock: Cells #({cellsText}) locks values {candidatesText}");

                foreach (Cell cell in cells)
                {
                    if (group.Contains(cell)) continue;
                    if (cell.RemoveCandidates(candidates, out error)) boardChanged = true;
                    if (error) return false;
                }
            }

            return boardChanged;
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