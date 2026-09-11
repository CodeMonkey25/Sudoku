namespace Sudoku;

public class CellState
{
    public bool IsGiven { get; set; }
    public int Candidates { get; set; } = 0b1_1111_1111;
}