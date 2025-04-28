using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI.SourceGenerators;
using Sudoku.Extensions;

namespace Sudoku.ViewModels;

public partial class SolverWindowViewModel : ViewModelBase
{
    [Reactive] private string _logText = string.Empty;
    private string PuzzleText { get; set; } = string.Empty;
    private int[] Puzzle { get; set; } = [];

    public void SolvePuzzle(string puzzleText)
    {
        PuzzleText = puzzleText;
        Puzzle = Board.ParsePuzzle(puzzleText);
        
        Task.Run(RunSolver);
    }

    private void RunSolver()
    {
        Stopwatch watch = Stopwatch.StartNew();
        Log("Received puzzle:");
        PrintPuzzle(Puzzle);
        
        Engine engine = new(Log);
        int[] solution = engine.Solve(Puzzle);
        PrintPuzzle(solution);
        
        watch.Stop();
        Log(string.Empty);
        Log($"Total run time: {watch.Elapsed.ReadableTime()}");
    }
    
    private void PrintPuzzle(int[] puzzle)
    {
        string rowDivider = new('-', 19);
        
        StringBuilder sb = new();
        sb.AppendLine(rowDivider);
        int cell = 1;
        foreach (int num in puzzle)
        {
            char c = (char)('0' + num);
            sb.Append('|');
            sb.Append(c == '0' ? ' ' : c);

            if (cell % 9 == 0)
            {
                sb.AppendLine("|");
                sb.AppendLine(rowDivider);
            }

            ++cell;
        }
        Log(sb.ToString());
    }
    
    public void Log(string message)
    {
        Dispatcher.UIThread.Post(() => LogText += message + Environment.NewLine);
    }
}