namespace Sudoku.Core.Tests;

public class EngineTests
{
    [Theory]
    [InlineData(nameof(Puzzles.L1N001))]
    [InlineData(nameof(Puzzles.L3N190))]
    [InlineData(nameof(Puzzles.L5N322))]
    public void Solve_ValidPuzzle_ReturnsValidSolution(string puzzleName)
    {
        int[] puzzle = Board.ParsePuzzle(PuzzlesTests.GetPuzzle(puzzleName));
        Engine engine = new();

        int[] solution = engine.Solve(puzzle, out string error);

        Assert.Equal(string.Empty, error);
        Assert.True(TestHelpers.IsValidSolution(solution));
        TestHelpers.AssertGivensPreserved(puzzle, solution);
    }

    [Fact]
    public void Solve_EmptyPuzzle_ReturnsValidSolution()
    {
        Engine engine = new();

        int[] solution = engine.Solve(new int[81], out string error);

        Assert.Equal(string.Empty, error);
        Assert.True(TestHelpers.IsValidSolution(solution));
    }

    [Fact]
    public void Solve_AlreadySolvedPuzzle_ReturnsSamePuzzle()
    {
        int[] puzzle = TestHelpers.SolvedGrid();
        Engine engine = new();

        int[] solution = engine.Solve(puzzle, out string error);

        Assert.Equal(string.Empty, error);
        Assert.Equal(puzzle, solution);
    }

    [Fact]
    public void Solve_DoesNotModifyInput()
    {
        int[] puzzle = Board.ParsePuzzle(Puzzles.L1N001);
        int[] copy = (int[])puzzle.Clone();

        new Engine().Solve(puzzle, out _);

        Assert.Equal(copy, puzzle);
    }

    [Fact]
    public void Solve_WrongLength_ReturnsErrorAndEmptyResult()
    {
        List<string> log = [];
        Engine engine = new(log.Add);

        int[] solution = engine.Solve(new int[80], out string error);

        Assert.Equal("Puzzle length does not match board length!", error);
        Assert.Empty(solution);
        Assert.Equal(["Malformed puzzle :-("], log);
    }

    [Fact]
    public void Solve_ConflictingGivens_ReturnsErrorAndEmptyResult()
    {
        int[] puzzle = new int[81];
        puzzle[0] = 9;
        puzzle[80] = 9;
        puzzle[8] = 9; // same row as cell 0

        int[] solution = new Engine().Solve(puzzle, out string error);

        Assert.NotEqual(string.Empty, error);
        Assert.Empty(solution);
    }

    [Fact]
    public void Solve_UnsolvablePuzzle_ReturnsError()
    {
        // givens are consistent, but grid 0 can only hold 1 and 2 in cell 0
        int[] puzzle = new int[81];
        puzzle[1 * 9 + 3] = 1;
        puzzle[2 * 9 + 6] = 1;
        puzzle[3 * 9 + 1] = 1;
        puzzle[6 * 9 + 2] = 1;
        puzzle[1 * 9 + 4] = 2;
        puzzle[2 * 9 + 7] = 2;
        puzzle[4 * 9 + 1] = 2;
        puzzle[7 * 9 + 2] = 2;
        List<string> log = [];
        Engine engine = new(log.Add);

        int[] solution = engine.Solve(puzzle, out string error);

        Assert.NotEqual(string.Empty, error);
        Assert.Equal(81, solution.Length);
        Assert.Contains(">>>>> Unable to find solution! <<<<<", log);
    }

    [Fact]
    public void Solve_WithLogger_LogsProgress()
    {
        List<string> log = [];
        Engine engine = new(log.Add);

        engine.Solve(Board.ParsePuzzle(Puzzles.L1N001), out string error);

        Assert.Equal(string.Empty, error);
        Assert.Contains("Initial setup", log);
        Assert.Contains("Solution found! :-)", log);
        Assert.Contains(log, static line => line.StartsWith("Number of guesses: "));
    }

    [Fact]
    public void Solve_HardPuzzle_LogsGuesses()
    {
        List<string> log = [];
        Engine engine = new(log.Add);

        engine.Solve(new int[81], out string error);

        Assert.Equal(string.Empty, error);
        Assert.Contains(log, static line => line.StartsWith("Guessing "));
    }

    [Fact]
    public void Solve_WithoutLogger_DoesNotThrow()
    {
        Engine engine = new();

        int[] solution = engine.Solve(Board.ParsePuzzle(Puzzles.L1N001), out string error);

        Assert.Equal(string.Empty, error);
        Assert.Equal(81, solution.Length);
    }

    [Fact]
    public void Solve_CanBeCalledRepeatedly()
    {
        Engine engine = new();
        int[] puzzle = Board.ParsePuzzle(Puzzles.L1N002);

        int[] first = engine.Solve(puzzle, out string firstError);
        int[] second = engine.Solve(puzzle, out string secondError);

        Assert.Equal(string.Empty, firstError);
        Assert.Equal(string.Empty, secondError);
        Assert.Equal(first, second);
    }
}
