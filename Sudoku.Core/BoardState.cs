namespace Sudoku;

public struct BoardState
{
    public CellState[] CellStates { get; init; } = [];
    
    public BoardState() { }
}