namespace Sudoku.Core.Tests;

public class BoardStateTests
{
    [Fact]
    public void Default_AllCellStatesAreEmpty()
    {
        BoardState state = default;

        for (int i = 0; i < 81; i++)
        {
            Assert.Equal(default, state[i]);
        }
    }

    [Fact]
    public void Indexer_StoresEachElementIndependently()
    {
        BoardState state = default;
        for (int i = 0; i < 81; i++)
        {
            state[i] = new CellState((ushort)(i + 1));
        }

        for (int i = 0; i < 81; i++)
        {
            Assert.Equal((ushort)(i + 1), state[i].Candidates);
        }
    }

    [Fact]
    public void Length_Is81()
    {
        BoardState state = default;
        Span<CellState> span = state;

        Assert.Equal(81, span.Length);
    }

    [Fact]
    public void Assignment_CopiesByValue()
    {
        BoardState original = default;
        original[0] = new CellState(1);

        BoardState copy = original;
        copy[0] = new CellState(2);

        Assert.Equal(1, original[0].Candidates);
        Assert.Equal(2, copy[0].Candidates);
    }
}
