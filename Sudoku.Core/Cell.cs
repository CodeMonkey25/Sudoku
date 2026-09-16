using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sudoku
{
    public sealed class Cell : IDisposable
    {
        public readonly int Index;
        public int Value;
        public bool IsGiven;

        private const int AllCandidatesMask = 0b1_1111_1111;

        private int _candidates = AllCandidatesMask;
        private readonly HashSet<Cell> _boundCells = new(24);

        public event Action<Cell, bool>? IsSolvedChanged;

        public bool IsSolved
        {
            get;
            private set
            {
                if (field == value) return;
                field = value;
                IsSolvedChanged?.Invoke(this, value);
            }
        }

        public Cell(int index)
        {
            Index = index;
        }

        public void Dispose()
        {
            ClearCandidates();
            _boundCells.Clear();
            IsSolvedChanged = null;
        }

        public void BindTo(Cell[] cells)
        {
            foreach (Cell cell in cells)
            {
                if (cell == this) continue;
                _boundCells.Add(cell);
            }
        }
        
        public void Solve(int value, out bool error)
        {
            error = false;
            if (IsSolved)
            {
                if (value != Value)
                {
                    // throw new Exception($"Cell {Index} is already solved with a different value: {Value} != {value}");
                    error = true;
                }

                return;
            }

            if (!HasCandidate(value))
            {
                // throw new Exception($"Value {value} is not valid for cell {Index}!");
                error = true;
                return;
            }

            ClearCandidates();
            AddCandidate(value);
            IsSolved = true;
            Value = value;

            foreach (Cell cell in _boundCells)
            {
                cell.RemoveCandidate(value, out error);
                if (error) break;
            }
        }
        
        public void ClearCandidates() => _candidates = 0;
        
        public int GetCandidateCount() => BitOperations.PopCount((uint)_candidates);

        public bool HasCandidate(int value) => (_candidates & (1 << (value - 1))) != 0;

        public int CandidateMask => _candidates;

        private static int GetCandidateCount(int mask) => BitOperations.PopCount((uint)mask);

        public static int[] GetCandidatesFromMask(int mask)
        {
            int[] candidates = new int[GetCandidateCount(mask)];
            int i = 0;
            while (mask != 0)
            {
                int bit = mask & -mask;
                candidates[i++] = BitOperations.TrailingZeroCount(bit) + 1;
                mask &= mask - 1;
            }
            return candidates;
        }
        
        public int[] GetCandidates() => GetCandidatesFromMask(_candidates);

        private void AddCandidate(int value) => _candidates |= 1 << (value - 1);

        private bool RemoveCandidate(int value, out bool error)
        {
            error = false;
            if (IsSolved)
            {
                // throw new Exception($"Cell {Index} - Attempting to remove solved value {value}!");
                error = value == Value;
                return false;
            }
            if (!HasCandidate(value)) return false;

            _candidates &= ~(1 << (value - 1));
            int remaining = GetCandidateCount();
            if (remaining == 0)
            {
                // throw new Exception($"Cell {Index} - No remaining candidates!");
                error = true;
                return false;
            }
            if (remaining == 1) Solve(BitOperations.TrailingZeroCount((uint)_candidates) + 1, out error);
            return true;
        }

        public bool RemoveCandidates(IReadOnlyList<int> candidates, out bool error)
        {
            bool changed = false;
            error = false;
            foreach (int candidate in candidates)
            {
                if (RemoveCandidate(candidate, out error)) changed = true;
                if (error) return false;
            }
            return changed;
        }

        public void Reset()
        {
            Value = 0;
            _candidates = AllCandidatesMask;
            IsSolved = false;
            IsGiven = false;
        }
        
        public CellState GetState()
        {
            return new CellState() { IsGiven = IsGiven, Candidates = _candidates, };
        }

        public void SetState(CellState state)
        {
            _candidates = state.Candidates;
            if (GetCandidateCount() == 1)
            {
                IsSolved = true;
                Value = GetCandidates()[0];
            }
            IsGiven = state.IsGiven;
        }
    }
}
