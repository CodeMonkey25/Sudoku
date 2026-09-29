namespace Sudoku;

public readonly record struct CellState(ushort Candidates); // not storing given flag since it never changes when solving puzzle
