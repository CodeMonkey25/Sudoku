using System;
using System.Collections.Generic;
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

        public int[] Solve(int[] puzzle, out string error)
        {
            error = string.Empty;
            using Board board = new();
            
            board.LoadPuzzle(puzzle, out error);
            if (!string.IsNullOrEmpty(error))
            {
                Log("Malformed puzzle :-(");
                return [];
            }
            PrintCandidates(board, "Initial setup");
            
            int guesses = 0;
            SolveIteratively(board, ref guesses, out error);
            
            if (!string.IsNullOrEmpty(error) || board.IsUnsolved())
            {
                string divider = new('*', 40);

                Log(divider);
                Log(">>>>> Unable to find solution! <<<<<");
                Log(divider);
                if (string.IsNullOrEmpty(error))
                {
                    error = "Unable to find solution!";
                }
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
                error = "Invalid solution found! :-(";
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

        private bool SolveLogically(Board board, out string error, Dictionary<int, List<Cell>> buffer)
        {
            error = string.Empty;
            Action<string> logAction = _log ?? (_ => { });
            bool boardChanged = true;
            bool printUpdate = false;
            
            // the cells are bound to one another when the board is created
            // when any cell is solved, it will notify the bound cells so they remove the solved value from their candidate list
            // if a remaining candidate list has only a single value, the cell declares itself solved and notifies its bound cells  
            // this does the majority of the work, but it does not solve every puzzle
            while (boardChanged && board.IsUnsolved())
            {
                boardChanged = false;
                
                // check for solved cells
                // this is obsolete -> the bound cells already notify each other when they are solved
                // boardChanged = boardChanged || board.CheckForSolvedCells();
                
                // check for cells with the only value for a row/col/grid
                // e.g. this row doesn't have a 9 yet, and this cell is the only one with a candidate for it
                if (board.CheckForLoneCandidates(logAction, out error)) printUpdate = boardChanged = true;
                if (!string.IsNullOrEmpty(error)) return false;
                
                // check for deadlocks
                if (board.CheckForDeadlockedCells(logAction, out error, buffer)) printUpdate = boardChanged = true;
                if (!string.IsNullOrEmpty(error)) return false;
            }

            if (printUpdate)
            {
                PrintCandidates(board, "Board State");
            }
            
            return board.IsSolved();
        }

        private bool SolveRecursively(Board board, ref int guesses, out string error, Dictionary<int, List<Cell>>? bufferMap = null, Stack<BoardState>? boardStatePool = null)
        {
            error = string.Empty;
            if (bufferMap == null) bufferMap = new Dictionary<int, List<Cell>>();
            if (boardStatePool == null) boardStatePool = new Stack<BoardState>();
            
            // try to solve the puzzle logically
            if (SolveLogically(board, out error, bufferMap)) return true;
            if (!string.IsNullOrEmpty(error)) return false;

            // try to guess the solution by checking candidates
            Cell cell = board.GetCellWithLeastAmountOfCandidates();
            Span<int> buffer = stackalloc int[9];
            foreach (int value in cell.GetCandidates(buffer))
            {
                BoardState state = boardStatePool.Count > 0 ? board.GetState(boardStatePool.Pop()) : board.GetState();
                guesses++;
                Log($"Guessing {value} for cell #{cell.Index}");
                cell.Solve(value, out error);
                if (string.IsNullOrEmpty(error))
                {
                    if (SolveRecursively(board, ref guesses, out error, bufferMap, boardStatePool))
                    {
                        if (string.IsNullOrEmpty(error)) return true;
                    }
                }

                Log("Failed to solve - Guess was bad! :-(");
                
                Log($"Reverting guess {value} for cell #{cell.Index}");
                board.RestoreState(state);
                boardStatePool.Push(state);
                guesses--;
            }
            return board.IsSolved();
        }
        
        private void SolveIteratively(Board board, ref int guesses, out string error)
        {
            error = string.Empty;
            
            Stack<LoopState> loopStates = new(board.Cells.Length);
            Stack<BoardState> boardStatePool = new(board.Cells.Length);
            Dictionary<int, List<Cell>> buffer = new();
            bool suppressLogicalSolve = false;
            do
            {
                if (!suppressLogicalSolve && SolveLogically(board, out error, buffer)) return;
                suppressLogicalSolve = false;
                
                LoopState loopState;
                Cell cell;
                if (!string.IsNullOrEmpty(error)) // an error means the guess was bad
                {
                    if (loopStates.Count == 0) return; // unable to solve and no more guesses
                    
                    error = string.Empty;
                    loopState = loopStates.Pop();
                    board.RestoreState(loopState.State);
                    cell = board.Cells[loopState.CellIndex];
                    guesses--;
                    Log("Failed to solve - Guess was bad! :-(");
                    Log($"Reverted guess {cell.GetCandidate(loopState.CandidatesIndex)} for cell #{cell.Index}");
                    
                    loopState.CandidatesIndex++;
                    if (loopState.CandidatesIndex >= loopState.CandidatesCount)
                    {
                        boardStatePool.Push(loopState.State);
                        error = "ran out of candidates";
                        suppressLogicalSolve = true;
                        continue; // ran out of candidates, previous guess was bad
                    }
                }
                else
                {
                    cell = board.GetCellWithLeastAmountOfCandidates();
                    BoardState boardState = boardStatePool.Count > 0 ? board.GetState(boardStatePool.Pop()) : board.GetState();
                    loopState = new LoopState(cell.Index, boardState, cell.GetCandidateCount(), 0);
                }
                
                int value = cell.GetCandidate(loopState.CandidatesIndex);
                
                Log($"Guessing {value} for cell #{cell.Index}");
                loopStates.Push(loopState);
                guesses++;
                cell.Solve(value, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    suppressLogicalSolve = true;
                }
                else
                {
                    if (board.IsSolved()) break;
                }
            } while (loopStates.Count > 0);
        }

        private record struct LoopState(int CellIndex, BoardState State, int CandidatesCount, int CandidatesIndex)
        {
            public int CellIndex { get; } = CellIndex;
            public BoardState State { get; } = State;
            public int CandidatesCount { get; } = CandidatesCount;
            public int CandidatesIndex { get; set; } = CandidatesIndex;
        }
    }
}
