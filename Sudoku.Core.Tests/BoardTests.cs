namespace Sudoku.Core.Tests;

public class BoardTests
{
    private const string MalformedMessage = "Puzzle is malformed: cell count is not 81";

    private static int[] Peers(int index)
    {
        int row = index / 9;
        int col = index % 9;
        int grid = row / 3 * 3 + col / 3;
        return Enumerable.Range(0, 81)
            .Where(i => i != index && (i / 9 == row || i % 9 == col || (i / 9 / 3 * 3 + i % 9 / 3) == grid))
            .ToArray();
    }

    private static Board LoadedBoard(string puzzle)
    {
        Board board = new();
        board.LoadPuzzle(Board.ParsePuzzle(puzzle), out string error);
        Assert.Equal(string.Empty, error);
        return board;
    }

    #region Construction

    [Fact]
    public void Constructor_Creates81IndexedUnsolvedCells()
    {
        using Board board = new();

        Assert.Equal(81, board.Cells.Length);
        for (int i = 0; i < board.Cells.Length; i++)
        {
            Assert.Equal(i, board.Cells[i].Index);
            Assert.False(board.Cells[i].IsSolved);
            Assert.Equal(9, board.Cells[i].GetCandidateCount());
        }
        Assert.True(board.IsUnsolved());
        Assert.False(board.IsSolved());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(40)]
    [InlineData(80)]
    [InlineData(30)]
    public void Constructor_BindsEachCellToItsRowColumnAndGrid(int index)
    {
        using Board board = new();
        HashSet<int> peers = [.. Peers(index)];

        board.Cells[index].Solve(7, out string error);

        Assert.Equal(string.Empty, error);
        Assert.Equal(20, peers.Count);
        foreach (Cell cell in board.Cells)
        {
            if (cell.Index == index) continue;
            Assert.Equal(!peers.Contains(cell.Index), cell.HasCandidate(7));
        }
    }

    #endregion

    #region ParsePuzzle

    [Fact]
    public void ParsePuzzle_SpaceSeparatedFormat_ReturnsValues()
    {
        int[] puzzle = Board.ParsePuzzle(Puzzles.L1N001);

        Assert.Equal(81, puzzle.Length);
        Assert.Equal([2, 0, 0, 1, 4, 5, 0, 0, 0], puzzle[..9]);
        Assert.Equal([0, 0, 0, 4, 5, 9, 0, 0, 2], puzzle[72..]);
    }

    [Fact]
    public void ParsePuzzle_CommaSeparatedFormat_ReturnsValues()
    {
        const string text = "4,,,,9,,,8,,,,,5,,,7,,,6,2,3,7,,,,4,,,4,9,,,,,7,3,,,,,,,,,,7,6,,,,,9,2,,,3,,,,2,4,1,5,,,2,,,6,,,,,1,,,5,,,,7";

        int[] puzzle = Board.ParsePuzzle(text);

        Assert.Equal(81, puzzle.Length);
        Assert.Equal([4, 0, 0, 0, 9, 0, 0, 8, 0], puzzle[..9]);
        Assert.Equal(7, puzzle[80]);
    }

    [Fact]
    public void ParsePuzzle_CommaSeparatedWithZeros_ReturnsValues()
    {
        string text = string.Join(",", Enumerable.Range(0, 81).Select(static i => i % 10));

        int[] puzzle = Board.ParsePuzzle(text);

        Assert.Equal(Enumerable.Range(0, 81).Select(static i => i % 10), puzzle);
    }

    [Fact]
    public void ParsePuzzle_CommaSeparated_IgnoresNonDigitCharacters()
    {
        string text = "." + new string(',', 80) + "x9";

        int[] puzzle = Board.ParsePuzzle(text);

        Assert.Equal(0, puzzle[0]);
        Assert.Equal(9, puzzle[80]);
    }

    [Fact]
    public void ParsePuzzle_SpaceSeparated_IgnoresNonDigitCharacters()
    {
        string text = "1 " + string.Concat(Enumerable.Repeat("0|", 79)) + " 9";

        int[] puzzle = Board.ParsePuzzle(text);

        Assert.Equal(1, puzzle[0]);
        Assert.Equal(9, puzzle[80]);
        Assert.All(puzzle[1..80], static v => Assert.Equal(0, v));
    }

    [Fact]
    public void ParsePuzzle_DigitsOnlyFormat_ReturnsValues()
    {
        const string text = "530070000600195000098000060800060003400803001700020006060000280000419005000080079";

        int[] puzzle = Board.ParsePuzzle(text);

        Assert.Equal(81, puzzle.Length);
        Assert.Equal([5, 3, 0, 0, 7, 0, 0, 0, 0], puzzle[..9]);
        Assert.Equal([0, 0, 0, 0, 8, 0, 0, 7, 9], puzzle[72..]);
    }

    [Fact]
    public void ParsePuzzle_DigitsOnlyFormat_MatchesSpaceSeparatedFormat()
    {
        string digitsOnly = string.Concat(Puzzles.L1N001.Where(char.IsDigit));

        Assert.Equal(Board.ParsePuzzle(Puzzles.L1N001), Board.ParsePuzzle(digitsOnly));
    }

    [Theory]
    [InlineData(80)]
    [InlineData(82)]
    public void ParsePuzzle_DigitsOnlyWrongLength_Throws(int digits)
    {
        string text = new('1', digits);

        Exception exception = Assert.Throws<Exception>(() => Board.ParsePuzzle(text));

        Assert.Equal(MalformedMessage, exception.Message);
    }

    [Theory]
    [InlineData('.')]
    [InlineData('x')]
    public void ParsePuzzle_DigitsOnlyLengthWithNonDigit_Throws(char nonDigit)
    {
        // 81 characters, but not all digits, and no spaces or commas to select another format
        string text = new string('0', 80) + nonDigit;

        Exception exception = Assert.Throws<Exception>(() => Board.ParsePuzzle(text));

        Assert.Equal(MalformedMessage, exception.Message);
    }

    [Theory]
    [InlineData("\n", "")]
    [InlineData("", "\r\n")]
    [InlineData("\r\n", "\n")]
    [InlineData("\t", "\t")]
    public void ParsePuzzle_DigitsOnlyWithSurroundingWhitespace_ReturnsValues(string prefix, string suffix)
    {
        string digits = string.Concat(Enumerable.Range(0, 81).Select(static i => i % 10));

        int[] puzzle = Board.ParsePuzzle(prefix + digits + suffix);

        Assert.Equal(digits.Select(static c => c - '0'), puzzle);
    }

    [Fact]
    public void ParsePuzzle_DigitsOnlyWithInnerNewLine_Throws()
    {
        string text = new string('0', 40) + "\n" + new string('0', 41);

        Exception exception = Assert.Throws<Exception>(() => Board.ParsePuzzle(text));

        Assert.Equal(MalformedMessage, exception.Message);
    }

    [Fact]
    public void ParsePuzzle_CommaSeparatedWithSpaces_UsesCommaFormat()
    {
        // e.g. "5, 3, , , 7, ..." - previously the spaces format was chosen and rejected the blank cells
        string[] cells = Enumerable.Repeat(" ", 81).ToArray();
        cells[0] = "5";
        cells[1] = " 3";
        cells[4] = " 7 ";
        string text = string.Join(",", cells);

        int[] puzzle = Board.ParsePuzzle(text);

        Assert.Equal([5, 3, 0, 0, 7, 0, 0, 0, 0], puzzle[..9]);
        Assert.All(puzzle[9..], static v => Assert.Equal(0, v));
    }

    [Fact]
    public void ParsePuzzle_CommasAndSpaces_CommaFormatTakesPrecedence()
    {
        // the spaces format would read "1 2" as two cells; the comma format keeps the last digit for the cell
        string text = "1 2" + new string(',', 80);

        int[] puzzle = Board.ParsePuzzle(text);

        Assert.Equal(2, puzzle[0]);
        Assert.All(puzzle[1..], static v => Assert.Equal(0, v));
    }

    [Fact]
    public void ParsePuzzle_CommaSeparatedWithNewLines_ReturnsValues()
    {
        string text = string.Join("," + Environment.NewLine, Enumerable.Range(0, 81).Select(static i => i % 9 + 1));

        int[] puzzle = Board.ParsePuzzle(text);

        Assert.Equal(Enumerable.Range(0, 81).Select(static i => i % 9 + 1), puzzle);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("123456789")]
    public void ParsePuzzle_UnrecognizedInput_Throws(string? text)
    {
        Exception exception = Assert.Throws<Exception>(() => Board.ParsePuzzle(text!));

        Assert.Equal(MalformedMessage, exception.Message);
    }

    [Theory]
    [InlineData(79)]
    [InlineData(81)]
    public void ParsePuzzle_WrongNumberOfCommas_Throws(int commas)
    {
        string text = new(',', commas);

        Exception exception = Assert.Throws<Exception>(() => Board.ParsePuzzle(text));

        Assert.Equal(MalformedMessage, exception.Message);
    }

    [Fact]
    public void ParsePuzzle_EmptyCommaSeparatedPuzzle_ReturnsZeros()
    {
        int[] puzzle = Board.ParsePuzzle(new string(',', 80));

        Assert.Equal(new int[81], puzzle);
    }

    [Theory]
    [InlineData(80)]
    [InlineData(82)]
    public void ParsePuzzle_WrongNumberOfDigitsWithSpaces_Throws(int digits)
    {
        string text = string.Join(" ", Enumerable.Repeat("0", digits));

        Exception exception = Assert.Throws<Exception>(() => Board.ParsePuzzle(text));

        Assert.Equal(MalformedMessage, exception.Message);
    }

    #endregion

    #region LoadPuzzle

    [Fact]
    public void LoadPuzzle_ValidPuzzle_SetsGivens()
    {
        int[] puzzle = Board.ParsePuzzle(Puzzles.L1N001);
        using Board board = new();

        board.LoadPuzzle(puzzle, out string error);

        Assert.Equal(string.Empty, error);
        for (int i = 0; i < puzzle.Length; i++)
        {
            if (puzzle[i] == 0) continue;
            Assert.True(board.Cells[i].IsSolved);
            Assert.Equal(puzzle[i], board.Cells[i].Value);
        }
    }

    [Fact]
    public void LoadPuzzle_MarksAllGivensAsGiven()
    {
        int[] puzzle = TestHelpers.SolvedGrid();
        using Board board = new();

        board.LoadPuzzle(puzzle, out string error);

        Assert.Equal(string.Empty, error);
        Assert.All(board.Cells, static cell => Assert.True(cell.IsGiven, $"Cell {cell.Index} should be a given"));
    }

    [Fact]
    public void LoadPuzzle_CellsSolvedByPropagation_AreNotGiven()
    {
        // row 0 has 1-8, so cell 8 is forced to 9 without being a given
        int[] puzzle = new int[81];
        for (int i = 0; i < 8; i++) puzzle[i] = i + 1;
        using Board board = new();

        board.LoadPuzzle(puzzle, out string error);

        Assert.Equal(string.Empty, error);
        Assert.True(board.Cells[8].IsSolved);
        Assert.Equal(9, board.Cells[8].Value);
        Assert.False(board.Cells[8].IsGiven);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(80)]
    [InlineData(82)]
    public void LoadPuzzle_WrongLength_ReturnsError(int length)
    {
        using Board board = new();

        board.LoadPuzzle(new int[length], out string error);

        Assert.Equal("Puzzle length does not match board length!", error);
    }

    [Fact]
    public void LoadPuzzle_ConflictingGivens_ReturnsError()
    {
        int[] puzzle = new int[81];
        puzzle[0] = 5;
        puzzle[1] = 5;
        using Board board = new();

        board.LoadPuzzle(puzzle, out string error);

        Assert.Equal("Value 5 is not valid for cell 1!", error);
    }

    [Fact]
    public void LoadPuzzle_ResetsPreviouslyLoadedPuzzle()
    {
        using Board board = LoadedBoard(Puzzles.L1N001);

        board.LoadPuzzle(new int[81], out string error);

        Assert.Equal(string.Empty, error);
        Assert.All(board.Cells, static cell =>
        {
            Assert.False(cell.IsSolved);
            Assert.False(cell.IsGiven);
            Assert.Equal(9, cell.GetCandidateCount());
        });
        Assert.True(board.IsUnsolved());
    }

    #endregion

    #region GetOriginalPuzzle / GetSolution

    [Fact]
    public void GetOriginalPuzzle_EmptyBoard_ReturnsOnlyCommas()
    {
        using Board board = new();

        Assert.Equal(new string(',', 80), board.GetOriginalPuzzle());
    }

    [Fact]
    public void GetOriginalPuzzle_RoundTripsThroughParsePuzzle()
    {
        using Board board = LoadedBoard(Puzzles.L1N001);

        int[] reparsed = Board.ParsePuzzle(board.GetOriginalPuzzle());

        Assert.Equal(board.GetSolution(), reparsed);
    }

    [Fact]
    public void GetOriginalPuzzle_FormatsSolvedValues()
    {
        int[] puzzle = new int[81];
        puzzle[0] = 3;
        puzzle[80] = 7;
        using Board board = new();
        board.LoadPuzzle(puzzle, out _);

        Assert.Equal("3" + new string(',', 80) + "7", board.GetOriginalPuzzle());
    }

    [Fact]
    public void GetSolution_ReturnsCellValuesWithZeroForUnsolved()
    {
        int[] puzzle = new int[81];
        puzzle[10] = 4;
        using Board board = new();
        board.LoadPuzzle(puzzle, out _);

        int[] solution = board.GetSolution();

        Assert.Equal(puzzle, solution);
    }

    #endregion

    #region State

    [Fact]
    public void GetState_CapturesEveryCell()
    {
        using Board board = LoadedBoard(Puzzles.L2N100);

        board.GetState(out BoardState state);

        for (int i = 0; i < 81; i++)
        {
            Assert.Equal(board.Cells[i].GetState(), state[i]);
        }
    }

    [Fact]
    public void RestoreState_RevertsChanges()
    {
        using Board board = LoadedBoard(Puzzles.L2N100);
        board.GetState(out BoardState state);
        int[] before = board.GetSolution();
        uint[] masksBefore = board.Cells.Select(static c => c.CandidatesMask).ToArray();

        Cell cell = board.GetCellWithLeastAmountOfCandidates();
        cell.Solve(cell.GetCandidate(0), out _);
        board.RestoreState(in state);

        Assert.Equal(before, board.GetSolution());
        Assert.Equal(masksBefore, board.Cells.Select(static c => c.CandidatesMask));
        Assert.True(board.IsUnsolved());
    }

    [Fact]
    public void RestoreState_AllSingleCandidates_MarksBoardSolved()
    {
        int[] grid = TestHelpers.SolvedGrid();
        BoardState state = TestHelpers.CreateBoardState(i => TestHelpers.MaskOf(grid[i]));
        using Board board = new();

        board.RestoreState(in state);

        Assert.True(board.IsSolved());
        Assert.False(board.IsUnsolved());
        Assert.Equal(grid, board.GetSolution());
    }

    [Fact]
    public void RestoreState_EmptyCellState_Throws()
    {
        BoardState state = default;
        using Board board = new();

        Assert.Throws<InvalidOperationException>(() => board.RestoreState(in state));
    }

    #endregion

    #region IsSolved / IsUnsolved / IsSolutionValid

    [Fact]
    public void IsSolved_TracksSolvedCellCount()
    {
        using Board board = new();
        board.LoadPuzzle(TestHelpers.SolvedGrid(), out _);

        Assert.True(board.IsSolved());
        Assert.False(board.IsUnsolved());

        board.LoadPuzzle(new int[81], out _);

        Assert.False(board.IsSolved());
        Assert.True(board.IsUnsolved());
    }

    [Fact]
    public void IsSolutionValid_ValidSolvedBoard_ReturnsTrue()
    {
        using Board board = new();
        board.LoadPuzzle(TestHelpers.SolvedGrid(), out _);

        Assert.True(board.IsSolutionValid());
    }

    [Fact]
    public void IsSolutionValid_UnsolvedBoard_ReturnsFalse()
    {
        using Board board = LoadedBoard(Puzzles.L2N100);

        Assert.False(board.IsSolutionValid());
    }

    [Fact]
    public void IsSolutionValid_DuplicateValues_ReturnsFalse()
    {
        BoardState state = TestHelpers.CreateBoardState(static _ => TestHelpers.MaskOf(1));
        using Board board = new();
        board.RestoreState(in state);

        Assert.True(board.IsSolved());
        Assert.False(board.IsSolutionValid());
    }

    [Fact]
    public void IsSolutionValid_ValidRowsAndColumnsButInvalidGrids_ReturnsFalse()
    {
        // latin square: rows and columns are valid, 3x3 grids are not
        BoardState state = TestHelpers.CreateBoardState(static i => TestHelpers.MaskOf((i / 9 + i % 9) % 9 + 1));
        using Board board = new();
        board.RestoreState(in state);

        Assert.False(board.IsSolutionValid());
    }

    #endregion

    #region CandidatesListing

    [Fact]
    public void CandidatesListing_EmptyBoard_ListsAllCandidatesForEachCell()
    {
        using Board board = new();

        string[] lines = board.CandidatesListing().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(81, lines.Length);
        Assert.Equal("00 => 1, 2, 3, 4, 5, 6, 7, 8, 9", lines[0]);
        Assert.Equal("09 => 1, 2, 3, 4, 5, 6, 7, 8, 9", lines[9]);
        Assert.Equal("10 => 1, 2, 3, 4, 5, 6, 7, 8, 9", lines[10]);
        Assert.Equal("80 => 1, 2, 3, 4, 5, 6, 7, 8, 9", lines[80]);
    }

    [Fact]
    public void CandidatesListing_ReflectsCurrentCandidates()
    {
        using Board board = new();
        board.Cells[0].Solve(5, out _);

        string[] lines = board.CandidatesListing().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("00 => 5", lines[0]);
        Assert.Equal("01 => 1, 2, 3, 4, 6, 7, 8, 9", lines[1]);
        Assert.Equal("40 => 1, 2, 3, 4, 5, 6, 7, 8, 9", lines[40]);
    }

    #endregion

    #region CheckForLoneCandidates

    [Fact]
    public void CheckForLoneCandidates_EmptyBoard_ReturnsFalse()
    {
        using Board board = new();
        List<string> log = [];

        bool changed = board.CheckForLoneCandidates(log.Add, out string error);

        Assert.False(changed);
        Assert.Equal(string.Empty, error);
        Assert.Empty(log);
    }

    [Fact]
    public void CheckForLoneCandidates_OnlyCellInRowWithValue_SolvesCell()
    {
        using Board board = new();
        for (int i = 1; i < 9; i++)
        {
            board.Cells[i].RemoveCandidates([5], out _);
        }
        List<string> log = [];

        bool changed = board.CheckForLoneCandidates(log.Add, out string error);

        Assert.True(changed);
        Assert.Equal(string.Empty, error);
        Assert.True(board.Cells[0].IsSolved);
        Assert.Equal(5, board.Cells[0].Value);
        Assert.Equal(["Lone Candidate: Cell #0 solved to 5"], log);
    }

    [Fact]
    public void CheckForLoneCandidates_OnlyCellInColumnWithValue_SolvesCell()
    {
        using Board board = new();
        for (int row = 0; row < 8; row++)
        {
            board.Cells[row * 9 + 4].RemoveCandidates([3], out _);
        }
        List<string> log = [];

        bool changed = board.CheckForLoneCandidates(log.Add, out string error);

        Assert.True(changed);
        Assert.Equal(string.Empty, error);
        Assert.Equal(3, board.Cells[76].Value);
        Assert.Equal(["Lone Candidate: Cell #76 solved to 3"], log);
    }

    [Fact]
    public void CheckForLoneCandidates_OnlyCellInGridWithValue_SolvesCell()
    {
        using Board board = new();
        int[] grid4 = [30, 31, 32, 39, 41, 48, 49, 50];
        foreach (int index in grid4)
        {
            board.Cells[index].RemoveCandidates([8], out _);
        }
        List<string> log = [];

        bool changed = board.CheckForLoneCandidates(log.Add, out string error);

        Assert.True(changed);
        Assert.Equal(string.Empty, error);
        Assert.Equal(8, board.Cells[40].Value);
        Assert.Equal(["Lone Candidate: Cell #40 solved to 8"], log);
    }

    [Fact]
    public void CheckForLoneCandidates_ResultsAgreeWithSolution()
    {
        int[] puzzle = Board.ParsePuzzle(Puzzles.L2N100);
        int[] solution = new Engine().Solve(puzzle, out _);
        using Board board = new();
        board.LoadPuzzle(puzzle, out _);
        int solvedBefore = board.Cells.Count(static c => c.IsSolved);

        bool changed = board.CheckForLoneCandidates(static _ => { }, out string error);

        Assert.True(changed);
        Assert.Equal(string.Empty, error);
        Assert.True(board.Cells.Count(static c => c.IsSolved) > solvedBefore);
        Assert.All(board.Cells.Where(static c => c.IsSolved), c => Assert.Equal(solution[c.Index], c.Value));
    }

    #endregion

    #region CheckForDeadlockedCells

    [Fact]
    public void CheckForDeadlockedCells_EmptyBoard_ReturnsFalse()
    {
        using Board board = new();
        List<string> log = [];

        bool changed = board.CheckForDeadlockedCells(log.Add, out string error);

        Assert.False(changed);
        Assert.Equal(string.Empty, error);
        Assert.Empty(log);
    }

    [Fact]
    public void CheckForDeadlockedCells_NakedPair_RemovesCandidatesFromRowAndGrid()
    {
        using Board board = new();
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[0], 1, 2);
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[1], 1, 2);
        List<string> log = [];

        bool changed = board.CheckForDeadlockedCells(log.Add, out string error);

        Assert.True(changed);
        Assert.Equal(string.Empty, error);
        Assert.Contains("Found deadlock: Cells #(0, 1) locks values 1, 2", log);

        // pair cells keep their candidates
        Assert.Equal([1, 2], board.Cells[0].GetCandidates());
        Assert.Equal([1, 2], board.Cells[1].GetCandidates());

        // rest of row 0 and grid 0 lose them
        int[] affected = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 18, 19, 20];
        Assert.All(affected, i =>
        {
            Assert.False(board.Cells[i].HasCandidate(1));
            Assert.False(board.Cells[i].HasCandidate(2));
        });

        // columns 0 and 1 outside of grid 0 are untouched
        Assert.True(board.Cells[27].HasCandidate(1));
        Assert.True(board.Cells[28].HasCandidate(2));
    }

    [Fact]
    public void CheckForDeadlockedCells_NakedTripleInColumn_RemovesCandidatesFromColumn()
    {
        using Board board = new();
        int[] triple = [4, 40, 76]; // column 4, different grids
        foreach (int index in triple)
        {
            TestHelpers.RemoveAllCandidatesExcept(board.Cells[index], 3, 6, 9);
        }
        List<string> log = [];

        bool changed = board.CheckForDeadlockedCells(log.Add, out string error);

        Assert.True(changed);
        Assert.Equal(string.Empty, error);
        Assert.Contains("Found deadlock: Cells #(4, 40, 76) locks values 3, 6, 9", log);
        for (int row = 0; row < 9; row++)
        {
            Cell cell = board.Cells[row * 9 + 4];
            if (triple.Contains(cell.Index)) continue;
            Assert.Equal([1, 2, 4, 5, 7, 8], cell.GetCandidates());
        }
        Assert.True(board.Cells[0].HasCandidate(3));
    }

    [Fact]
    public void CheckForDeadlockedCells_SingleCellWithPairMask_ReturnsFalse()
    {
        using Board board = new();
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[0], 1, 2);
        List<string> log = [];

        bool changed = board.CheckForDeadlockedCells(log.Add, out string error);

        Assert.False(changed);
        Assert.Equal(string.Empty, error);
        Assert.Empty(log);
    }

    [Fact]
    public void CheckForDeadlockedCells_ConflictingDeadlocks_ReturnsError()
    {
        // the {1, 2} pair leaves cells 3 and 4 both limited to {3}
        using Board board = new();
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[0], 1, 2);
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[1], 1, 2);
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[3], 1, 2, 3);
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[4], 1, 2, 3);

        bool changed = board.CheckForDeadlockedCells(static _ => { }, out string error);

        Assert.False(changed);
        Assert.NotEqual(string.Empty, error);
    }

    [Fact]
    public void CheckForDeadlockedCells_ResultsAgreeWithSolution()
    {
        int[] puzzle = Board.ParsePuzzle(Puzzles.L2N100);
        int[] solution = new Engine().Solve(puzzle, out _);
        using Board board = new();
        board.LoadPuzzle(puzzle, out _);

        bool changed = board.CheckForDeadlockedCells(static _ => { }, out string error);

        Assert.True(changed);
        Assert.Equal(string.Empty, error);
        Assert.All(board.Cells, c => Assert.True(c.HasCandidate(solution[c.Index])));
    }

    #endregion

    #region GetCellWithLeastAmountOfCandidates

    [Fact]
    public void GetCellWithLeastAmountOfCandidates_EmptyBoard_ReturnsFirstCell()
    {
        using Board board = new();

        Assert.Same(board.Cells[0], board.GetCellWithLeastAmountOfCandidates());
    }

    [Fact]
    public void GetCellWithLeastAmountOfCandidates_ReturnsCellWithFewestCandidates()
    {
        using Board board = new();
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[40], 1, 2, 3);
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[70], 4, 5, 6, 7);

        Assert.Same(board.Cells[40], board.GetCellWithLeastAmountOfCandidates());
    }

    [Fact]
    public void GetCellWithLeastAmountOfCandidates_IgnoresSolvedCells()
    {
        using Board board = new();
        board.Cells[0].Solve(1, out _);
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[80], 5, 6);

        Assert.Same(board.Cells[80], board.GetCellWithLeastAmountOfCandidates());
    }

    [Fact]
    public void GetCellWithLeastAmountOfCandidates_TieReturnsFirstCell()
    {
        using Board board = new();
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[50], 5, 6, 7);
        TestHelpers.RemoveAllCandidatesExcept(board.Cells[60], 1, 2, 3);

        Assert.Same(board.Cells[50], board.GetCellWithLeastAmountOfCandidates());
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_DisposesAllCells()
    {
        Board board = new();

        board.Dispose();

        Assert.All(board.Cells, static cell => Assert.Equal(0u, cell.CandidatesMask));
    }

    [Fact]
    public void Dispose_UnbindsCells()
    {
        Board board = new();

        board.Dispose();
        board.Cells[0].Reset();
        board.Cells[1].Reset();
        board.Cells[0].Solve(1, out _);

        Assert.True(board.Cells[1].HasCandidate(1));
    }

    #endregion
}
