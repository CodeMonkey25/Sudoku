namespace Sudoku;

public readonly record struct CellState(bool IsGiven, int Candidates);
