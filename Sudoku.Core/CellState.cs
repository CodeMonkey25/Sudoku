using System.Collections.Generic;

namespace Sudoku;

public class CellState
{
    public bool IsGiven { get; set; }
    public HashSet<int> Candidates { get; set; } = [];
}