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

        private readonly HashSet<int> _candidates = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        private readonly List<Cell> _boundCells = new(24);

        public void Dispose()
        {
            _candidates.Clear();
            _boundCells.Clear();
        }

        public void BindTo(Cell[] cells)
        {
            foreach (Cell cell in cells)
            {
                if (cell == this) continue;
                if (_boundCells.Contains(cell)) continue;
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

            if (!_candidates.Contains(value))
            {
                throw new Exception($"Value {value} is not valid for cell {Index}!");
            }

            _candidates.Clear();
            _candidates.Add(value);
            IsSolved = true;
            Value = value;

            foreach (Cell cell in _boundCells)
            {
                cell.RemoveCandidate(value);
            }
        }
        
        public int CountCandidates() => _candidates.Count;
        
        public bool HasCandidate(int value) => _candidates.Contains(value);
        
        public IEnumerable<int> GetCandidates() => _candidates;

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

            if (!_candidates.Remove(value)) return false;

            if (_candidates.Count == 0) throw new Exception($"Cell {Index} - No remaining candidates!");
            if (_candidates.Count == 1) Solve(_candidates.First());
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
            _candidates.Clear();
            _candidates.UnionWith([1, 2, 3, 4, 5, 6, 7, 8, 9]);
            IsSolved = false;
            IsGiven = false;
        }
        
        public CellState GetState()
        {
            return new CellState()
            {
                Candidates = _candidates.ToHashSet(),
                IsGiven = IsGiven,
            };
        }

        public void SetState(CellState state)
        {
            _candidates.Clear();
            _candidates.UnionWith(state.Candidates);
            if (_candidates.Count == 1)
            {
                IsSolved = true;
                Value = _candidates.First();
            }
            IsGiven = state.IsGiven;
        }
    }
}