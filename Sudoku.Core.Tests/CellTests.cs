namespace Sudoku.Core.Tests;

public class CellTests
{
    private const uint AllCandidatesMask = 0b1_1111_1111;

    private static Cell[] CreateBoundGroup(int count)
    {
        Cell[] cells = Enumerable.Range(0, count).Select(static i => new Cell(i)).ToArray();
        foreach (Cell cell in cells)
        {
            cell.BindTo(cells);
        }
        return cells;
    }

    #region Construction

    [Fact]
    public void Constructor_InitializesUnsolvedCellWithAllCandidates()
    {
        Cell cell = new(42);

        Assert.Equal(42, cell.Index);
        Assert.Equal(0, cell.Value);
        Assert.False(cell.IsSolved);
        Assert.False(cell.IsGiven);
        Assert.Equal(AllCandidatesMask, cell.CandidatesMask);
        Assert.Equal(9, cell.GetCandidateCount());
        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9], cell.GetCandidates());
    }

    #endregion

    #region Candidates

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(9)]
    public void HasCandidate_NewCell_ReturnsTrue(int value)
    {
        Assert.True(new Cell(0).HasCandidate(value));
    }

    [Fact]
    public void HasCandidate_RemovedValue_ReturnsFalse()
    {
        Cell cell = new(0);
        cell.RemoveCandidates([4], out _);

        Assert.False(cell.HasCandidate(4));
        Assert.True(cell.HasCandidate(3));
        Assert.True(cell.HasCandidate(5));
    }

    [Fact]
    public void ClearCandidates_RemovesAllCandidates()
    {
        Cell cell = new(0);

        cell.ClearCandidates();

        Assert.Equal(0u, cell.CandidatesMask);
        Assert.Equal(0, cell.GetCandidateCount());
        Assert.Empty(cell.GetCandidates());
    }

    [Theory]
    [InlineData(0u, new int[0])]
    [InlineData(0b1u, new[] { 1 })]
    [InlineData(0b1_0000_0000u, new[] { 9 })]
    [InlineData(0b1_0000_0101u, new[] { 1, 3, 9 })]
    [InlineData(AllCandidatesMask, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
    public void GetCandidatesFromMask_ReturnsAscendingValues(uint mask, int[] expected)
    {
        Span<int> buffer = stackalloc int[9];

        int[] actual = Cell.GetCandidatesFromMask(mask, buffer).ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetCandidates_WithBuffer_ReturnsCurrentCandidates()
    {
        Cell cell = new(0);
        cell.RemoveCandidates([1, 3, 5, 7], out _);
        Span<int> buffer = stackalloc int[9];

        int[] actual = cell.GetCandidates(buffer).ToArray();

        Assert.Equal([2, 4, 6, 8, 9], actual);
    }

    [Fact]
    public void GetCandidates_Array_ReturnsCurrentCandidates()
    {
        Cell cell = new(0);
        cell.RemoveCandidates([2, 4, 6, 8], out _);

        Assert.Equal([1, 3, 5, 7, 9], cell.GetCandidates());
    }

    [Fact]
    public void GetCandidate_ReturnsCandidateAtPosition()
    {
        Cell cell = new(0);
        cell.RemoveCandidates([1, 2, 5], out _);

        Assert.Equal(3, cell.GetCandidate(0));
        Assert.Equal(4, cell.GetCandidate(1));
        Assert.Equal(6, cell.GetCandidate(2));
        Assert.Equal(9, cell.GetCandidate(5));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void GetCandidate_IndexOutOfRange_Throws(int index)
    {
        Cell cell = new(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => cell.GetCandidate(index));
    }

    [Fact]
    public void GetCandidate_IndexBeyondRemainingCandidates_Throws()
    {
        Cell cell = new(0);
        cell.RemoveCandidates([1, 2, 3, 4, 5, 6, 7], out _);

        Assert.Throws<ArgumentOutOfRangeException>(() => cell.GetCandidate(2));
    }

    #endregion

    #region RemoveCandidates

    [Fact]
    public void RemoveCandidates_ExistingCandidates_ReturnsTrue()
    {
        Cell cell = new(0);

        bool changed = cell.RemoveCandidates([1, 2], out string error);

        Assert.True(changed);
        Assert.Equal(string.Empty, error);
        Assert.Equal([3, 4, 5, 6, 7, 8, 9], cell.GetCandidates());
    }

    [Fact]
    public void RemoveCandidates_MissingCandidates_ReturnsFalse()
    {
        Cell cell = new(0);
        cell.RemoveCandidates([1], out _);

        bool changed = cell.RemoveCandidates([1], out string error);

        Assert.False(changed);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void RemoveCandidates_Empty_ReturnsFalse()
    {
        Cell cell = new(0);

        bool changed = cell.RemoveCandidates([], out string error);

        Assert.False(changed);
        Assert.Equal(string.Empty, error);
        Assert.Equal(AllCandidatesMask, cell.CandidatesMask);
    }

    [Fact]
    public void RemoveCandidates_LeavingSingleCandidate_SolvesCell()
    {
        Cell cell = new(0);

        bool changed = cell.RemoveCandidates([1, 2, 3, 4, 5, 6, 8, 9], out string error);

        Assert.True(changed);
        Assert.Equal(string.Empty, error);
        Assert.True(cell.IsSolved);
        Assert.Equal(7, cell.Value);
        Assert.False(cell.IsGiven);
    }

    [Fact]
    public void RemoveCandidates_SolvedValue_ReturnsError()
    {
        Cell cell = new(3);
        cell.Solve(5, out _);

        bool changed = cell.RemoveCandidates([5], out string error);

        Assert.False(changed);
        Assert.Equal("Cell 3 - Attempting to remove solved value 5!", error);
    }

    [Fact]
    public void RemoveCandidates_AllCandidates_SolvesThenReportsError()
    {
        Cell cell = new(0);

        bool changed = cell.RemoveCandidates([1, 2, 3, 4, 5, 6, 7, 8, 9], out string error);

        Assert.False(changed);
        Assert.Equal("Cell 0 - Attempting to remove solved value 9!", error);
    }

    [Fact]
    public void RemoveCandidates_OtherValueFromSolvedCell_ReturnsFalseWithoutError()
    {
        Cell cell = new(0);
        cell.Solve(5, out _);

        bool changed = cell.RemoveCandidates([4], out string error);

        Assert.False(changed);
        Assert.Equal(string.Empty, error);
        Assert.Equal(5, cell.Value);
    }

    #endregion

    #region Solve

    [Fact]
    public void Solve_ValidCandidate_SolvesCell()
    {
        Cell cell = new(0);

        cell.Solve(4, out string error);

        Assert.Equal(string.Empty, error);
        Assert.True(cell.IsSolved);
        Assert.Equal(4, cell.Value);
        Assert.False(cell.IsGiven);
        Assert.Equal([4], cell.GetCandidates());
    }

    [Fact]
    public void Solve_AsGiven_SetsIsGiven()
    {
        Cell cell = new(0);

        cell.Solve(4, out _, isGiven: true);

        Assert.True(cell.IsGiven);
    }

    [Fact]
    public void Solve_NonCandidate_ReturnsError()
    {
        Cell cell = new(7);
        cell.RemoveCandidates([3], out _);

        cell.Solve(3, out string error);

        Assert.Equal("Value 3 is not valid for cell 7!", error);
        Assert.False(cell.IsSolved);
        Assert.Equal(0, cell.Value);
    }

    [Fact]
    public void Solve_AlreadySolvedWithSameValue_NoError()
    {
        Cell cell = new(0);
        cell.Solve(6, out _);

        cell.Solve(6, out string error);

        Assert.Equal(string.Empty, error);
        Assert.Equal(6, cell.Value);
    }

    [Fact]
    public void Solve_AlreadySolvedWithDifferentValue_ReturnsError()
    {
        Cell cell = new(2);
        cell.Solve(6, out _);

        cell.Solve(7, out string error);

        Assert.Equal("Cell 2 is already solved with a different value: 6 != 7", error);
        Assert.Equal(6, cell.Value);
    }

    [Fact]
    public void Solve_AlreadySolvedWithSameValueAsGiven_SetsIsGiven()
    {
        Cell cell = new(0);
        cell.Solve(6, out _);

        cell.Solve(6, out string error, isGiven: true);

        Assert.Equal(string.Empty, error);
        Assert.True(cell.IsGiven);
    }

    [Fact]
    public void Solve_GivenCellSolvedAgainWithoutGivenFlag_RemainsGiven()
    {
        Cell cell = new(0);
        cell.Solve(6, out _, isGiven: true);

        cell.Solve(6, out string error);

        Assert.Equal(string.Empty, error);
        Assert.True(cell.IsGiven);
    }

    [Fact]
    public void Solve_AlreadySolvedWithDifferentValueAsGiven_DoesNotSetIsGiven()
    {
        Cell cell = new(0);
        cell.Solve(6, out _);

        cell.Solve(7, out string error, isGiven: true);

        Assert.NotEqual(string.Empty, error);
        Assert.False(cell.IsGiven);
    }

    [Fact]
    public void Solve_RaisesIsSolvedChanged()
    {
        Cell cell = new(0);
        List<(Cell Cell, bool IsSolved)> events = [];
        cell.IsSolvedChanged += (c, solved) => events.Add((c, solved));

        cell.Solve(1, out _);
        cell.Solve(1, out _);

        Assert.Equal([(cell, true)], events);
    }

    [Fact]
    public void Solve_RemovesValueFromBoundCells()
    {
        Cell[] cells = CreateBoundGroup(3);

        cells[0].Solve(5, out string error);

        Assert.Equal(string.Empty, error);
        Assert.False(cells[1].HasCandidate(5));
        Assert.False(cells[2].HasCandidate(5));
        Assert.Equal(8, cells[1].GetCandidateCount());
    }

    [Fact]
    public void Solve_CascadesThroughBoundCells()
    {
        Cell[] cells = CreateBoundGroup(3);
        TestHelpers.RemoveAllCandidatesExcept(cells[1], 5, 6);

        cells[0].Solve(5, out string error);

        Assert.Equal(string.Empty, error);
        Assert.True(cells[1].IsSolved);
        Assert.Equal(6, cells[1].Value);
        Assert.False(cells[2].HasCandidate(5));
        Assert.False(cells[2].HasCandidate(6));
    }

    [Fact]
    public void Solve_BoundCellAlreadySolvedWithSameValue_ReturnsError()
    {
        Cell first = new(0);
        Cell second = new(1);
        first.BindTo([first, second]); // one-way binding so second does not notify first
        second.Solve(5, out _);

        first.Solve(5, out string error);

        Assert.Equal("Cell 1 - Attempting to remove solved value 5!", error);
    }

    [Fact]
    public void Solve_DoesNotAffectUnboundCells()
    {
        Cell[] cells = CreateBoundGroup(2);
        Cell unbound = new(99);

        cells[0].Solve(5, out _);

        Assert.True(unbound.HasCandidate(5));
    }

    #endregion

    #region BindTo

    [Fact]
    public void BindTo_IgnoresSelfAndDuplicates()
    {
        Cell[] group = Enumerable.Range(0, 9).Select(static i => new Cell(i)).ToArray();
        Cell[] others = Enumerable.Range(9, 11).Select(static i => new Cell(i)).ToArray();

        // 8 cells from group + 11 others = 19 distinct bindings (<= 20), binding group twice must not overflow
        group[0].BindTo(group);
        group[0].BindTo(group);
        group[0].BindTo(others);
    }

    [Fact]
    public void BindTo_TwentyDistinctCells_Succeeds()
    {
        Cell cell = new(0);
        Cell[] others = Enumerable.Range(1, 20).Select(static i => new Cell(i)).ToArray();

        cell.BindTo(others);
        cell.Solve(1, out string error);

        Assert.Equal(string.Empty, error);
        Assert.All(others, static other => Assert.False(other.HasCandidate(1)));
    }

    [Fact]
    public void BindTo_MoreThanTwentyDistinctCells_Throws()
    {
        Cell cell = new(0);
        Cell[] others = Enumerable.Range(1, 21).Select(static i => new Cell(i)).ToArray();

        Exception exception = Assert.Throws<Exception>(() => cell.BindTo(others));

        Assert.Equal("_boundedCells overflow", exception.Message);
    }

    #endregion

    #region Reset

    [Fact]
    public void Reset_RestoresInitialState()
    {
        Cell cell = new(0);
        cell.Solve(3, out _, isGiven: true);

        cell.Reset();

        Assert.Equal(0, cell.Value);
        Assert.False(cell.IsSolved);
        Assert.False(cell.IsGiven);
        Assert.Equal(AllCandidatesMask, cell.CandidatesMask);
    }

    [Fact]
    public void Reset_SolvedCell_RaisesIsSolvedChanged()
    {
        Cell cell = new(0);
        cell.Solve(3, out _);
        List<bool> events = [];
        cell.IsSolvedChanged += (_, solved) => events.Add(solved);

        cell.Reset();
        cell.Reset();

        Assert.Equal([false], events);
    }

    #endregion

    #region State

    [Fact]
    public void GetState_ReturnsCandidatesMask()
    {
        Cell cell = new(0);
        cell.RemoveCandidates([1, 9], out _);

        CellState state = cell.GetState();

        Assert.Equal((ushort)cell.CandidatesMask, state.Candidates);
        Assert.Equal(TestHelpers.MaskOf(2, 3, 4, 5, 6, 7, 8), state.Candidates);
    }

    [Fact]
    public void SetState_MultipleCandidates_LeavesCellUnsolved()
    {
        Cell cell = new(0);
        cell.Solve(1, out _);

        cell.SetState(new CellState(TestHelpers.MaskOf(2, 5, 8)));

        Assert.False(cell.IsSolved);
        Assert.Equal(0, cell.Value);
        Assert.Equal([2, 5, 8], cell.GetCandidates());
    }

    [Fact]
    public void SetState_SingleCandidate_SolvesCell()
    {
        Cell cell = new(0);

        cell.SetState(new CellState(TestHelpers.MaskOf(8)));

        Assert.True(cell.IsSolved);
        Assert.Equal(8, cell.Value);
    }

    [Fact]
    public void SetState_NoCandidates_Throws()
    {
        Cell cell = new(4);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => cell.SetState(default));

        Assert.Equal("Cell 4 - state has no candidates.", exception.Message);
    }

    [Fact]
    public void SetState_RaisesIsSolvedChangedOnTransitions()
    {
        Cell cell = new(0);
        List<bool> events = [];
        cell.IsSolvedChanged += (_, solved) => events.Add(solved);

        cell.SetState(new CellState(TestHelpers.MaskOf(1)));
        cell.SetState(new CellState(TestHelpers.MaskOf(2)));
        cell.SetState(new CellState(TestHelpers.MaskOf(1, 2)));

        Assert.Equal([true, false], events);
    }

    [Fact]
    public void GetState_SetState_RoundTrips()
    {
        Cell source = new(0);
        source.RemoveCandidates([2, 4, 6], out _);
        Cell target = new(0);

        target.SetState(source.GetState());

        Assert.Equal(source.CandidatesMask, target.CandidatesMask);
        Assert.Equal(source.IsSolved, target.IsSolved);
        Assert.Equal(source.Value, target.Value);
    }

    [Fact]
    public void SetState_DoesNotPropagateToBoundCells()
    {
        Cell[] cells = CreateBoundGroup(2);

        cells[0].SetState(new CellState(TestHelpers.MaskOf(3)));

        Assert.True(cells[1].HasCandidate(3));
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_ClearsCandidates()
    {
        Cell cell = new(0);

        cell.Dispose();

        Assert.Equal(0u, cell.CandidatesMask);
    }

    [Fact]
    public void Dispose_UnsubscribesHandlers()
    {
        Cell cell = new(0);
        int raised = 0;
        cell.IsSolvedChanged += (_, _) => raised++;

        cell.Dispose();
        cell.Reset();
        cell.Solve(1, out _);

        Assert.Equal(0, raised);
    }

    [Fact]
    public void Dispose_ClearsBindings()
    {
        Cell[] cells = CreateBoundGroup(2);

        cells[0].Dispose();
        cells[0].Reset();
        cells[0].Solve(1, out _);

        Assert.True(cells[1].HasCandidate(1));
    }

    #endregion
}
