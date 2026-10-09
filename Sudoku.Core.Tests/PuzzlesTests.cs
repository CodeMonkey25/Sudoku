using System.Reflection;

namespace Sudoku.Core.Tests;

public class PuzzlesTests
{
    private static readonly FieldInfo[] PuzzleFields = typeof(Puzzles)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(static f => f.IsLiteral && f.FieldType == typeof(string))
        .ToArray();

    public static TheoryData<string> PuzzleNames => new(PuzzleFields.Select(static f => f.Name));

    public static string GetPuzzle(string name) => (string)typeof(Puzzles).GetField(name)!.GetRawConstantValue()!;

    [Fact]
    public void Puzzles_ContainsPuzzles()
    {
        Assert.NotEmpty(PuzzleFields);
    }

    [Theory]
    [MemberData(nameof(PuzzleNames))]
    public void Puzzle_ParsesTo81Cells(string name)
    {
        int[] puzzle = Board.ParsePuzzle(GetPuzzle(name));

        Assert.Equal(81, puzzle.Length);
        Assert.All(puzzle, static v => Assert.InRange(v, 0, 9));
    }

    [Theory]
    [MemberData(nameof(PuzzleNames))]
    public void Puzzle_IsSolvable(string name)
    {
        int[] puzzle = Board.ParsePuzzle(GetPuzzle(name));

        int[] solution = new Engine().Solve(puzzle, out string error);

        Assert.Equal(string.Empty, error);
        Assert.True(TestHelpers.IsValidSolution(solution));
        TestHelpers.AssertGivensPreserved(puzzle, solution);
    }
}
