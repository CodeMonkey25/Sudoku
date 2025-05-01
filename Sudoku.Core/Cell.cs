using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public sealed class Cell(int index) : IDisposable
    {
        public readonly int Index = index;
        public bool IsSolved;
        public int Value;
        public bool IsGiven;

        public readonly HashSet<int> Candidates = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        private readonly List<Cell> _boundCells = new(24);

        public void Dispose()
        {
            Candidates.Clear();
            _boundCells.Clear();
        }

        public void BindTo(Cell[] cells)
        {
            foreach (Cell cell in cells)
            {
                if (cell == this) continue;
                _boundCells.Add(cell);
            }
        }
        
        public void Solve(int value)
        {
            if (IsSolved)
            {
                if (value != Value)
                {
                    throw new Exception($"Cell {Index} is already solved with a different value: {Value} != {value}");
                }

                return;
            }

            if (!Candidates.Contains(value))
            {
                throw new Exception($"Value {value} is not valid for cell {Index}!");
            }

            Candidates.Clear();
            Candidates.Add(value);
            IsSolved = true;
            Value = value;

            foreach (Cell cell in _boundCells)
            {
                cell.RemoveCandidate(value);
            }
        }

        private bool RemoveCandidate(int value)
        {
            if (IsSolved)
            {
                if (value == Value)
                {
                    throw new Exception($"Cell {Index} - Attempting to remove solved value {value}!");
                }

                return false;
            }

            if (!Candidates.Remove(value)) return false;

            if (Candidates.Count == 0) throw new Exception($"Cell {Index} - No remaining candidates!");
            if (Candidates.Count == 1) Solve(Candidates.First());
            return true;
        }

        public bool RemoveCandidates(IEnumerable<int> candidates)
        {
            bool cellChanged = false;
            foreach (int candidate in candidates.ToArray())
            {
                if (RemoveCandidate(candidate)) cellChanged = true;
            }
            return cellChanged;
        }

        public void Reset()
        {
            Value = 0;
            Candidates.Clear();
            Candidates.UnionWith([1, 2, 3, 4, 5, 6, 7, 8, 9]);
            IsSolved = false;
            IsGiven = false;
        }
        
        public CellState GetState()
        {
            return new CellState()
            {
                Candidates = Candidates.ToHashSet(),
                IsGiven = IsGiven,
            };
        }

        public void SetState(CellState state)
        {
            Candidates.Clear();
            Candidates.UnionWith(state.Candidates);
            if (Candidates.Count == 1)
            {
                IsSolved = true;
                Value = Candidates.First();
            }
            IsGiven = state.IsGiven;
        }
    }
}