using ReactiveUI.SourceGenerators;

namespace Sudoku.ViewModels;

public partial class BoardViewModel : ViewModelBase
{
    [Reactive] private Board _board = new();

    public void LoadPuzzle(string puzzleText)
    {
        int[] puzzle = Board.ParsePuzzle(puzzleText);
        _board.LoadPuzzle(puzzle);
    }

    public string GetPuzzle()
    {
        return Board.GetOriginalPuzzle();
    }
}