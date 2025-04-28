using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class Engine
    {
        private Action<string> _log;

        public Engine(Action<string> log)
        {
            _log = log;
        }

        public int[] Solve(int[] puzzle)
        {
            using Board board = new();
            
            board.LoadPuzzle(puzzle);
            PrintCandidates(board, "Initial setup");
            
            int guesses = 0;
            _solveLoop(board, ref guesses);
            
            if (board.IsUnsolved())
            {
                string divider = new('*', 40);

                _log(divider);
                _log(">>>>> Unable to find solution! <<<<<");
                _log(divider);
            }
            else if (board.IsSolutionValid())
            {
                _log(string.Empty);
                _log("Solution found! :-)");
            }
            else
            {
                _log(string.Empty);
                _log("Invalid solution found! :-(");
            }
            _log($"Number of guesses: {guesses}");

            _log(string.Empty);
            return board.GetSolution();
        }

        private void PrintCandidates(Board board, string message)
        {
            string debugSpacer = new('*', 40);
            
            _log(string.Empty);
            _log(debugSpacer);
            _log(string.Empty);
            _log(message);
            _log(board.CandidatesListing());
            _log(debugSpacer);
            _log(string.Empty);
        }

        private bool _solveLoop(Board board, ref int guesses)
        {
            // try to solve the puzzle logically
            if (_solveLogically(board)) return true;

            // try to guess the solution by checking candidates
            Cell cell = board.GetCellWithLeastAmountOfCandidates();
            foreach (int value in cell.Candidates.ToArray())
            {
                HashSet<int>[] state = board.GetState();
                guesses++;
                try
                {
                    _log($"Guessing {value} for cell #{cell.Index}");
                    cell.Solve(value);
                    if (_solveLoop(board, ref guesses)) return true;
                }
                catch
                {
                    _log("Failed to solve - Guess was bad! :-(");
                }

                _log($"Reverting guess {value} for cell #{cell.Index}");
                board.RestoreState(state);
                guesses--;
            }
            return board.IsSolved();
        }

        private bool _solveLogically(Board board)
        {
            // the cells are bound to one another when the board is created
            // when any cell is solved, it will notify the bound cells so they remove the solved value from their candidate list
            // if a remaining candidate list has only a single value, the cell declares itself solved and notifies its bound cells  
            // this does the majority of the work, but it does not solve every puzzle
            while (board.IsUnsolved())
            {
                bool boardChanged = false;
                
                // we are using the logical or operators below to only run methods until the board changes, then skip the others
                
                // check for solved cells
                // this is obsolete -> the bound cells already notify each other when they are solved
                // boardChanged = boardChanged || board.CheckForSolvedCells();
                
                // check for cells with the only value for a row/col/grid
                // e.g. this row doesn't have a 9 yet, and this cell is the only one with a candidate for it
                boardChanged = boardChanged || board.CheckForLoneCandidates(_log);
                
                // check for deadlocks
                boardChanged = boardChanged || board.CheckForDeadlockedCells(_log);

                PrintCandidates(board, "Board State");
                if (!boardChanged) break;
            }

            return board.IsSolved();
        }
    }
}