using Sudoku.Utility;

namespace Sudoku.Core.Tests.Utility;

public class ArrayEqualityComparerTests
{
    private readonly ArrayEqualityComparer<int> _comparer = new();

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        int[] array = [1, 2, 3];

        Assert.True(_comparer.Equals(array, array));
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        Assert.True(_comparer.Equals(null, null));
    }

    [Fact]
    public void Equals_OneNull_ReturnsFalse()
    {
        Assert.False(_comparer.Equals([1], null));
        Assert.False(_comparer.Equals(null, [1]));
    }

    [Fact]
    public void Equals_SameElements_ReturnsTrue()
    {
        Assert.True(_comparer.Equals([1, 2, 3], [1, 2, 3]));
        Assert.True(_comparer.Equals([], []));
    }

    [Fact]
    public void Equals_DifferentLength_ReturnsFalse()
    {
        Assert.False(_comparer.Equals([1, 2], [1, 2, 3]));
    }

    [Fact]
    public void Equals_DifferentOrder_ReturnsFalse()
    {
        Assert.False(_comparer.Equals([1, 2, 3], [3, 2, 1]));
    }

    [Fact]
    public void Equals_ReferenceTypeElements_UsesDefaultEquality()
    {
        ArrayEqualityComparer<string> comparer = new();

        Assert.True(comparer.Equals(["a", "b"], [new string('a', 1), "b"]));
        Assert.False(comparer.Equals(["a", "b"], ["a", "c"]));
    }

    [Fact]
    public void GetHashCode_Null_ReturnsZero()
    {
        Assert.Equal(0, _comparer.GetHashCode(null));
    }

    [Fact]
    public void GetHashCode_EqualArrays_ReturnSameHash()
    {
        Assert.Equal(_comparer.GetHashCode([4, 5, 6]), _comparer.GetHashCode([4, 5, 6]));
    }

    [Fact]
    public void GetHashCode_DifferentOrder_ReturnsDifferentHash()
    {
        Assert.NotEqual(_comparer.GetHashCode([1, 2]), _comparer.GetHashCode([2, 1]));
    }

    [Fact]
    public void Comparer_WorksAsHashSetComparer()
    {
        HashSet<int[]> set = new(_comparer) { new[] { 1, 2, 3 } };

        Assert.Contains(new[] { 1, 2, 3 }, set);
        Assert.False(set.Add([1, 2, 3]));
    }
}
