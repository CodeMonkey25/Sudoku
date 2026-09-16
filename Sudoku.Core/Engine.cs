using System;
using System.Linq;

namespace Sudoku
{
    public class Engine
    {
        private readonly Action<string>? _log;

        public Engine(Action<string>? log = null)
        {
            _log = log;
        }

        private void Log(string message)
        {
            _log?.Invoke(message);
        }

        public int[] Solve(int[] puzzle, out bool error)
        {
            error = false;
            using Board board = new();
            
            board.LoadPuzzle(puzzle, out error);
            if (error)
            {
                Log("Malformed puzzle :-(");
                return [];
            }
            PrintCandidates(board, "Initial setup");
            
            int guesses = 0;
            _solveLoop(board, ref guesses, out error);
            
            if (error || board.IsUnsolved())
            {
                string divider = new('*', 40);

                Log(divider);
                Log(">>>>> Unable to find solution! <<<<<");
                Log(divider);
            }
            else if (board.IsSolutionValid())
            {
                Log(string.Empty);
                Log("Solution found! :-)");
            }
            else
            {
                Log(string.Empty);
                Log("Invalid solution found! :-(");
            }
            Log($"Number of guesses: {guesses}");

            Log(string.Empty);
            return board.GetSolution();
        }

        private void PrintCandidates(Board board, string message)
        {
            if (_log == null) return;
            string debugSpacer = new('*', 40);

            Log(string.Empty);
            Log(debugSpacer);
            Log(string.Empty);
            Log(message);
            Log(board.CandidatesListing());
            Log(debugSpacer);
            Log(string.Empty);
        }

        private bool _solveLoop(Board board, ref int guesses, out bool error)
        {
            error = false;
            // try to solve the puzzle logically
            if (_solveLogically(board, out error)) return true;
            if (error) return false;

            // try to guess the solution by checking candidates
            Cell cell = board.GetCellWithLeastAmountOfCandidates();
            foreach (int value in cell.GetCandidates().ToArray())
            {
                BoardState state = board.GetState();
                guesses++;
                Log($"Guessing {value} for cell #{cell.Index}");
                cell.Solve(value, out error);
                if (!error)
                {
                    if (_solveLoop(board, ref guesses, out error))
                    {
                        if (!error) return true;
                    }
                }

                Log("Failed to solve - Guess was bad! :-(");
                
                Log($"Reverting guess {value} for cell #{cell.Index}");
                board.RestoreState(state);
                guesses--;
            }
            return board.IsSolved();
        }

        private bool _solveLogically(Board board, out bool error)
        {
            error = false;
            Action<string> logAction = _log ?? (_ => { });
            
            // the cells are bound to one another when the board is created
            // when any cell is solved, it will notify the bound cells so they remove the solved value from their candidate list
            // if a remaining candidate list has only a single value, the cell declares itself solved and notifies its bound cells  
            // this does the majority of the work, but it does not solve every puzzle
            while (board.IsUnsolved())
            {
                bool boardChanged = false;
                
                // check for solved cells
                // this is obsolete -> the bound cells already notify each other when they are solved
                // boardChanged = boardChanged || board.CheckForSolvedCells();
                
                // check for cells with the only value for a row/col/grid
                // e.g. this row doesn't have a 9 yet, and this cell is the only one with a candidate for it
                if (board.CheckForLoneCandidates(logAction, out error)) boardChanged = true;
                if (error) return false;
                
                // check for deadlocks
                if (board.CheckForDeadlockedCells(logAction, out error)) boardChanged = true;
                if (error) return false;

                PrintCandidates(board, "Board State");
                if (!boardChanged) break;
            }

            return board.IsSolved();
        }
    }
}