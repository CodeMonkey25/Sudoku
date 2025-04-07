using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class Cell(int index) : IDisposable
    {
        public int Index { get; } = index;
        public bool IsSolved { get; private set; }
        public int Value { get; private set; }
        public bool IsGiven { get; set; }

        public HashSet<int> Candidates { get; } = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        private HashSet<Cell> BoundCells { get; } = new(24);

        public void Dispose()
        {
            Candidates.Clear();
            BoundCells.Clear();
        }

        public void BindTo(Cell[] cells)
        {
            for (var i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                if (cell == this) continue;
                BoundCells.Add(cell);
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

            foreach (Cell boundCell in BoundCells)
            {
                boundCell.RemoveCandidate(value);
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