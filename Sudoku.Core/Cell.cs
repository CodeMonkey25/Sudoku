using System;
using System.Numerics;

namespace Sudoku
{
    public sealed class Cell : IDisposable
    {
        private const uint AllCandidatesMask = 0b1_1111_1111;
        
        public int Index { get; }
        public int Value { get; private set; }
        public bool IsGiven { get; private set; }
        public uint CandidatesMask { get; private set; } = AllCandidatesMask;
        
        private readonly Cell?[] _boundCells = new Cell?[20];

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
            Array.Clear(_boundCells);
            IsSolvedChanged = null;
        }

        public void BindTo(Cell[] cells)
        {
            foreach (Cell cell in cells)
            {
                if (cell == this) continue;
                
                bool added = false;
                for (int i = 0; i < _boundCells.Length; i++)
                {
                    if (_boundCells[i] == cell)
                    {
                        added = true;
                        break;
                    }
                    
                    if (_boundCells[i] == null)
                    {
                        _boundCells[i] = cell;
                        added = true;
                        break;
                    }
                }

                if (!added) throw new Exception("_boundedCells overflow");
            }
        }
        
        public void Solve(int value, out string error, bool isGiven = false)
        {
            error = string.Empty;
            if (IsSolved)
            {
                if (value != Value)
                {
                    error = $"Cell {Index} is already solved with a different value: {Value} != {value}";
                    return;
                }

                // a given may already be solved by propagation from earlier givens; never clear an existing given
                IsGiven = IsGiven || isGiven;
                return;
            }

            if (!HasCandidate(value))
            {
                error = $"Value {value} is not valid for cell {Index}!";
                return;
            }

            ClearCandidates();
            AddCandidate(value);
            Value = value;
            IsSolved = true;
            IsGiven = isGiven;

            foreach (Cell? cell in _boundCells)
            {
                cell?.RemoveCandidate(value, out error);
                if (!string.IsNullOrEmpty(error)) break;
            }
        }
        
        public void ClearCandidates() => CandidatesMask = 0;
        
        public int GetCandidateCount() => BitOperations.PopCount(CandidatesMask);

        public bool HasCandidate(int value) => (CandidatesMask & (1u << (value - 1))) != 0;

        public static ReadOnlySpan<int> GetCandidatesFromMask(uint mask, Span<int> buffer)
        {
            int i = 0;
            while (mask != 0)
            {
                buffer[i++] = BitOperations.TrailingZeroCount(mask) + 1;
                mask &= mask - 1;
            }
            return buffer.Slice(0, i);
        }
        
        public ReadOnlySpan<int> GetCandidates(Span<int> buffer) => GetCandidatesFromMask(CandidatesMask, buffer);
        
        public int[] GetCandidates() => GetCandidates(stackalloc int[9]).ToArray();
        
        public int GetCandidate(int index)
        {
            uint mask = CandidatesMask;
            int count = 0;
            while (mask != 0)
            {
                if (count == index) return BitOperations.TrailingZeroCount(mask) + 1;
                mask &= mask - 1;
                count++;
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        private void AddCandidate(int value) => CandidatesMask |= 1u << (value - 1);

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

            CandidatesMask &= ~(1u << (value - 1));
            int remaining = GetCandidateCount();
            if (remaining == 0)
            {
                error = $"Cell {Index} - No remaining candidates!";
                return false;
            }
            if (remaining == 1) Solve(BitOperations.TrailingZeroCount(CandidatesMask) + 1, out error);
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
            CandidatesMask = AllCandidatesMask;
            IsSolved = false;
            IsGiven = false;
        }
        
        public CellState GetState() => new((ushort)CandidatesMask);

        public void SetState(CellState state)
        {
            CandidatesMask = state.Candidates;
            int count = BitOperations.PopCount(CandidatesMask);
            switch (count)
            {
                case 0:
                    throw new InvalidOperationException($"Cell {Index} - state has no candidates.");
                case 1:
                    Value = BitOperations.TrailingZeroCount(CandidatesMask) + 1;
                    IsSolved = true;
                    break;
                default:
                    Value = 0;
                    IsSolved = false;
                    break;
            }
        }
    }
}
