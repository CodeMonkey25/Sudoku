namespace Sudoku.Core.Tests;

public class CellStateTests
{
    [Fact]
    public void Constructor_SetsCandidates()
    {
        CellState state = new(0b1_0101);

        Assert.Equal(0b1_0101, state.Candidates);
    }

    [Fact]
    public void Default_HasNoCandidates()
    {
        CellState state = default;

        Assert.Equal(0, state.Candidates);
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        Assert.Equal(new CellState(7), new CellState(7));
        Assert.NotEqual(new CellState(7), new CellState(8));
        Assert.True(new CellState(7) == new CellState(7));
        Assert.Equal(new CellState(7).GetHashCode(), new CellState(7).GetHashCode());
    }
}
