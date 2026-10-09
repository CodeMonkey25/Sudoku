using Sudoku.Utility;

namespace Sudoku.Core.Tests.Utility;

public class SetEqualityComparerTests
{
    private readonly SetEqualityComparer<int> _comparer = new();

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        HashSet<int> set = [1, 2];

        Assert.True(_comparer.Equals(set, set));
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        Assert.True(_comparer.Equals(null, null));
    }

    [Fact]
    public void Equals_OneNull_ReturnsFalse()
    {
        Assert.False(_comparer.Equals(new HashSet<int> { 1 }, null));
        Assert.False(_comparer.Equals(null, new HashSet<int> { 1 }));
    }

    [Fact]
    public void Equals_SameElements_ReturnsTrue()
    {
        Assert.True(_comparer.Equals(new HashSet<int> { 1, 2, 3 }, new HashSet<int> { 1, 2, 3 }));
    }

    [Fact]
    public void Equals_SameElementsDifferentInsertionOrder_ReturnsTrue()
    {
        Assert.True(_comparer.Equals(new HashSet<int> { 1, 2, 3 }, new HashSet<int> { 3, 2, 1 }));
    }

    [Fact]
    public void Equals_DifferentElements_ReturnsFalse()
    {
        Assert.False(_comparer.Equals(new HashSet<int> { 1, 2, 3 }, new HashSet<int> { 1, 2, 4 }));
    }

    [Fact]
    public void Equals_DifferentCount_ReturnsFalse()
    {
        Assert.False(_comparer.Equals(new HashSet<int> { 1, 2 }, new HashSet<int> { 1, 2, 3 }));
    }

    [Fact]
    public void Equals_DifferentSetTypes_ReturnsFalse()
    {
        Assert.False(_comparer.Equals(new HashSet<int> { 1, 2 }, new SortedSet<int> { 1, 2 }));
    }

    [Fact]
    public void Equals_DifferentReadOnlyness_ReturnsFalse()
    {
        ConfigurableReadOnlySet readOnly = new([1, 2], isReadOnly: true);
        ConfigurableReadOnlySet writable = new([1, 2], isReadOnly: false);

        Assert.False(_comparer.Equals(readOnly, writable));
    }

    [Fact]
    public void GetHashCode_Null_ReturnsZero()
    {
        Assert.Equal(0, _comparer.GetHashCode(null));
    }

    [Fact]
    public void GetHashCode_EqualSets_ReturnSameHash()
    {
        Assert.Equal(_comparer.GetHashCode(new HashSet<int> { 1, 2, 3 }), _comparer.GetHashCode(new HashSet<int> { 1, 2, 3 }));
    }

    [Fact]
    public void GetHashCode_SetsEqualByComparer_ReturnSameHash()
    {
        // IEqualityComparer contract: Equals(x, y) implies GetHashCode(x) == GetHashCode(y)
        HashSet<int> first = [1, 2, 3];
        HashSet<int> second = [3, 2, 1];

        Assert.True(_comparer.Equals(first, second));
        Assert.Equal(_comparer.GetHashCode(first), _comparer.GetHashCode(second));
    }

    private sealed class ConfigurableReadOnlySet(IEnumerable<int> items, bool isReadOnly) : HashSet<int>(items), ISet<int>
    {
        bool ICollection<int>.IsReadOnly => isReadOnly;
    }
}
