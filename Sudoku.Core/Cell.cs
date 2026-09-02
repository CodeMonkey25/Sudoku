using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public sealed class Cell : IDisposable
    {
        public readonly int Index;
        public bool IsSolved;
        public int Value;
        public bool IsGiven;

        private readonly bool[] _candidates = new bool[9];
        private readonly List<Cell> _boundCells = new(24);

        public Cell(int index)
        {
            Index = index;
            Array.Fill(_candidates, true);
        }

        public void Dispose()
        {
            ClearCandidates();
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

            if (!HasCandidate(value))
            {
                throw new Exception($"Value {value} is not valid for cell {Index}!");
            }

            ClearCandidates();
            AddCandidate(value);
            IsSolved = true;
            Value = value;

            foreach (Cell cell in _boundCells)
            {
                cell.RemoveCandidate(value);
            }
        }
        
        public void ClearCandidates() => Array.Clear(_candidates);
        
        public int CountCandidates()
        {
            int count = 0;
            foreach (bool b in _candidates)
            {
                if (b) count++;
            }
            return count;
        }

        public bool HasCandidate(int value) => _candidates[value - 1];
        
        public IEnumerable<int> GetCandidates()
        {
            for (int i = 0; i < _candidates.Length; i++)
            {
                if (_candidates[i]) yield return i + 1;
            }
        }

        private void AddCandidate(int value) => _candidates[value - 1] = true;

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

            if (!HasCandidate(value)) return false;

            _candidates[value - 1] = false;
            if (CountCandidates() == 0) throw new Exception($"Cell {Index} - No remaining candidates!");
            if (CountCandidates() == 1) Solve(GetCandidates().First());
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
            Array.Fill(_candidates, true);
            IsSolved = false;
            IsGiven = false;
        }
        
        public CellState GetState()
        {
            CellState state = new() { IsGiven = IsGiven, };
            Array.Copy(_candidates, state.Candidates, state.Candidates.Length);
            return state;
        }

        public void SetState(CellState state)
        {
            Array.Copy(state.Candidates, _candidates, state.Candidates.Length);
            if (_candidates.Length == 1)
            {
                IsSolved = true;
                Value = GetCandidates().First();
            }
            IsGiven = state.IsGiven;
        }
    }
}