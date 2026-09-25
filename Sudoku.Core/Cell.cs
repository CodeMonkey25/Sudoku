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
        
        public void Solve(int value, out string error)
        {
            error = string.Empty;
            if (IsSolved)
            {
                if (value != Value)
                {
                    error = $"Cell {Index} is already solved with a different value: {Value} != {value}";
                }

                return;
            }

            if (!HasCandidate(value))
            {
                error = $"Value {value} is not valid for cell {Index}!";
                return;
            }

            ClearCandidates();
            AddCandidate(value);
            IsSolved = true;
            Value = value;

            foreach (Cell cell in _boundCells)
            {
                cell.RemoveCandidate(value, out error);
                if (!string.IsNullOrEmpty(error)) break;
            }
        }
        
        public void ClearCandidates() => _candidates = 0;
        
        public int GetCandidateCount() => BitOperations.PopCount((uint)_candidates);

        public bool HasCandidate(int value) => (_candidates & (1 << (value - 1))) != 0;

        public int CandidateMask => _candidates;

        public static ReadOnlySpan<int> GetCandidatesFromMask(int mask, Span<int> buffer)
        {
            int i = 0;
            while (mask != 0)
            {
                int bit = mask & -mask;
                buffer[i++] = BitOperations.TrailingZeroCount(bit) + 1;
                mask &= mask - 1;
            }
            return buffer.Slice(0, i);
        }
        
        public ReadOnlySpan<int> GetCandidates(Span<int> buffer) => GetCandidatesFromMask(_candidates, buffer);
        
        public int[] GetCandidates() => GetCandidatesFromMask(_candidates, stackalloc int[9]).ToArray();
        
        public int GetCandidate(int index)
        {
            int mask = _candidates;
            int count = 0;
            while (mask != 0)
            {
                int bit = mask & -mask;
                if (count == index) return BitOperations.TrailingZeroCount(bit) + 1;
                mask &= mask - 1;
                count++;
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        private void AddCandidate(int value) => _candidates |= 1 << (value - 1);

        private bool RemoveCandidate(int value, out string error)
        {
            error = string.Empty;
            if (IsSolved)
            {
                if (value == Value)
                {
                    error = $"Cell {Index} - Attempting to remove solved value {value}!";
                }
                return false;
            }
            if (!HasCandidate(value)) return false;

            _candidates &= ~(1 << (value - 1));
            int remaining = GetCandidateCount();
            if (remaining == 0)
            {
                error = $"Cell {Index} - No remaining candidates!";
                return false;
            }
            if (remaining == 1) Solve(BitOperations.TrailingZeroCount((uint)_candidates) + 1, out error);
            return true;
        }

        public bool RemoveCandidates(ReadOnlySpan<int> candidates, out string error)
        {
            bool changed = false;
            error = string.Empty;
            foreach (int candidate in candidates)
            {
                if (RemoveCandidate(candidate, out error)) changed = true;
                if (!string.IsNullOrEmpty(error)) return false;
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
        
        public CellState GetState() => new(IsGiven, _candidates);

        public void SetState(CellState state)
        {
            _candidates = state.Candidates;
            ReadOnlySpan<int> candidates = GetCandidates(stackalloc int[9]);
            switch (candidates.Length)
            {
                case 0:
                    throw new InvalidOperationException($"Cell {Index} - state has no candidates.");
                case 1:
                    IsSolved = true;
                    Value = candidates[0];
                    break;
                default:
                    IsSolved = false;
                    Value = 0;
                    break;
            }
            IsGiven = state.IsGiven;
        }
    }
}
