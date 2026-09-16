namespace Sudoku;

public struct CellState
{
    public bool IsGiven { get; set; } = false;
    public int Candidates { get; set; } = 0b1_1111_1111;
    
    public CellState() { }
}