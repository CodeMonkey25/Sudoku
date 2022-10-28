using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class Cell : IDisposable
    {
        public int Index { get; }
        public bool IsSolved { get; private set; }
        public int Value { get; private set; }

        public int[] GetCandidates() => Candidates.ToArray();
        
        private HashSet<int> Candidates { get; } = Enumerable.Range(1, 9).ToHashSet();
        private HashSet<Cell> BoundCells { get; } = new();
        
        public Cell(int index)
        {
            Index = index;
        }

        public void Dispose()
        {
            Candidates.Clear();
            BoundCells.Clear();
        }

        public void BindTo(IEnumerable<Cell> cells)
        {
            foreach (Cell cell in cells)
            {
                if (cell == this) continue;
                BindTo(cell);
            }
        }
        
        public void BindTo(Cell cell)
        {
            BoundCells.Add(cell);
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

            foreach (Cell boundCell in BoundCells)
            {
                boundCell.RemoveCandidate(value);
            }
        }

        public bool RemoveCandidate(int value)
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

            int count = Candidates.Count;
            if (count == 0) throw new Exception($"Cell {Index} - No remaining candidates!");
            if (count == 1) Solve(Candidates.First());
            return true;
        }

        public bool RemoveCandidates(IEnumerable<int> candidates)
        {
            bool cellChanged = false;
            foreach (int candidate in candidates)
            {
                if (RemoveCandidate(candidate)) cellChanged = true;
            }
            return cellChanged;
        }
        
        // public bool AttemptToSolve()
        // {
        //     if (IsSolved) return false;
        //     if (Candidates.Count != 1) return false;
        //     
        //     Solve(Candidates.First());
        //     
        //     return true;
        // }
    }
}