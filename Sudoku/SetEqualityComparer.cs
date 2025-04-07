using System.Collections.Generic;

namespace Sudoku;

public sealed class SetEqualityComparer<T> : IEqualityComparer<ISet<T>> where T : notnull
{
    private static readonly EqualityComparer<T> ElementComparer = EqualityComparer<T>.Default;

    public bool Equals(ISet<T>? first, ISet<T>? second)
    {
        if (ReferenceEquals(first, second)) return true;
        if (first is null) return false;
        if (second is null) return false;
        if (first.GetType() != second.GetType()) return false;
        if (first.Count != second.Count) return false;
        if (first.IsReadOnly != second.IsReadOnly) return false;
        
        foreach (T item in first)
        {
            if (!second.Contains(item)) return false;
        }
        
        return true;
    }

    public int GetHashCode(ISet<T>? set)
    {
        if (set == null) return 0;
        unchecked
        {
            int hash = 17;
            foreach (T element in set)
            {
                hash = hash * 31 + ElementComparer.GetHashCode(element);
            }
            return hash;
        }
    }
}