namespace Sudoku.Core.Tests;

internal static class TestHelpers
{
    public static bool IsValidSolution(int[] solution)
    {
        if (solution.Length != 81) return false;

        for (int i = 0; i < 9; i++)
        {
            HashSet<int> row = [];
            HashSet<int> column = [];
            HashSet<int> grid = [];
            for (int j = 0; j < 9; j++)
            {
                row.Add(solution[i * 9 + j]);
                column.Add(solution[j * 9 + i]);
                grid.Add(solution[(i / 3 * 3 + j / 3) * 9 + (i % 3 * 3 + j % 3)]);
            }

            if (!IsCompleteGroup(row) || !IsCompleteGroup(column) || !IsCompleteGroup(grid)) return false;
        }

        return true;
    }

    private static bool IsCompleteGroup(HashSet<int> values) => values.Count == 9 && values.All(static v => v is >= 1 and <= 9);

    public static void AssertGivensPreserved(int[] puzzle, int[] solution)
    {
        for (int i = 0; i < puzzle.Length; i++)
        {
            if (puzzle[i] != 0) Assert.Equal(puzzle[i], solution[i]);
        }
    }

    public static BoardState CreateBoardState(Func<int, ushort> candidates)
    {
        BoardState state = default;
        for (int i = 0; i < 81; i++)
        {
            state[i] = new CellState(candidates(i));
        }
        return state;
    }

    public static ushort MaskOf(params int[] values)
    {
        ushort mask = 0;
        foreach (int value in values)
        {
            mask |= (ushort)(1 << (value - 1));
        }
        return mask;
    }

    public static void RemoveAllCandidatesExcept(Cell cell, params int[] keep)
    {
        int[] remove = Enumerable.Range(1, 9).Except(keep).ToArray();
        cell.RemoveCandidates(remove, out string error);
        Assert.Equal(string.Empty, error);
    }

    // a solved board (sudoku with a well-known cyclic pattern)
    public static int[] SolvedGrid() => Enumerable.Range(0, 81)
        .Select(static i => (i / 9 * 3 + i / 27 + i % 9) % 9 + 1)
        .ToArray();
}
