using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class Cell(int index) : IDisposable
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
            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] == this) continue;
                _boundCells.Add(cells[i]);
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

            for (int i = 0; i < _boundCells.Count; i++)
            {
                _boundCells[i].RemoveCandidate(value);
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