using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sudoku
{
    public static class Engine
    {
        public static int[] Solve(int[] puzzle)
        {
            using Board board = new();
            
            board.LoadPuzzle(puzzle);
            PrintCandidates(board, "Initial setup");
            
            int pass = 0;
            _solveLoop(board, ref pass);
            
            if (board.IsUnsolved())
            {
                string divider = new('*', 40);

                Debug.WriteLine(divider);
                Debug.WriteLine(">>>>> Unable to find solution! <<<<<");
                Debug.WriteLine(divider);
            }
            else if (board.IsSolutionValid())
            {
                Debug.WriteLine("Solution found! :-)");
            }
            else
            {
                Debug.WriteLine("Invalid solution found! :-(");
            }
            Debug.WriteLine($"Number of passes: {pass}");

            Debug.WriteLine(string.Empty);
            return board.GetSolution();
        }

        private static void PrintCandidates(Board board, string message)
        {
            string debugSpacer = new('*', 40);
            
            Debug.WriteLine(string.Empty);
            Debug.WriteLine(debugSpacer);
            Debug.WriteLine(string.Empty);
            Debug.WriteLine(message);
            Debug.WriteLine(board.CandidatesListing());
            Debug.WriteLine(debugSpacer);
            Debug.WriteLine(string.Empty);
        }

        private static bool _solveLoop(Board board, ref int pass)
        {
            // try to solve the puzzle logically
            if (_solveLogically(board, ref pass)) return true;

            // try to guess the solution by checking candidates
            Cell cell = board.GetCellWithLeastAmountOfCandidates();
            foreach (int value in cell.Candidates.ToArray())
            {
                HashSet<int>[] state = board.GetState();
                int savePass = pass;
                try
                {
                    cell.Solve(value);
                    if (_solveLoop(board, ref pass)) return true;
                }
                catch
                {
                    // ignored
                }

                board.RestoreState(state);
                pass = savePass;
            }
            return board.IsSolved();
        }

        private static bool _solveLogically(Board board, ref int pass)
        {
            // the cells are bound to one another when the board is created
            // when any cell is solved, it will notify the bound cells so they remove the solved value from their candidate list
            // if a remaining candidate list has only a single value, the cell declares itself solved and notifies its bound cells  
            // this does the majority of the work, but it does not solve every puzzle
            while (board.IsUnsolved())
            {
                pass++;
                bool boardChanged = false;
                
                // we are using the logical or operators below to only run methods until the board changes, then skip the others
                
                // check for solved cells
                // this is obsolete -> the bound cells already notify each other when they are solved
                // boardChanged = boardChanged || board.CheckForSolvedCells();
                
                // check for cells with the only value for a row/col/grid
                // e.g. this row doesn't have a 9 yet, and this cell is the only one with a candidate for it
                boardChanged = boardChanged || board.CheckForLoneCandidates();
                
                // check for deadlocks
                boardChanged = boardChanged || board.CheckForDeadlockedCells();

                PrintCandidates(board, $"Pass #{pass}");
                if (!boardChanged) break;
            }

            return board.IsSolved();
        }
    }
}