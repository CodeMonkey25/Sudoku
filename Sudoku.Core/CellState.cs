using System.Collections.Generic;

namespace Sudoku;

public class CellState
{
    public bool IsGiven { get; set; }
    public bool[] Candidates { get; set; } = new bool[9];
}