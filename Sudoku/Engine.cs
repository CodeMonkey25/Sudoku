using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class Engine
    {
        public int[] Solve(int[] puzzle)
        {
            HashSet<int>[] candidates = CreateCandidates(puzzle);
            
            
            
            return GetSolution(candidates);
        }

        private HashSet<int>[] CreateCandidates(int[] puzzle)
        {
            return Enumerable.Range(0, 81)
                .Select(i => puzzle[i] == 0 ? Enumerable.Range(1, 9).ToHashSet() : new() { puzzle[i] })
                .ToArray();
        }

        private int[] GetSolution(HashSet<int>[] candidates)
        {
            return candidates
                .Select(hs => hs.Count == 1 ? hs.First() : 0)
                .ToArray();
        }
    }
}