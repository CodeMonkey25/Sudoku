using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Utility;

public sealed class SetEqualityComparer<T> : IEqualityComparer<ISet<T>> where T : notnull
{
    public bool Equals(ISet<T>? first, ISet<T>? second)
    {
        if (object.ReferenceEquals(first, second)) return true;
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
        
        HashCode hashCode = new();
        foreach (T item in set.Order())
        {
            hashCode.Add(item);
        }
        return hashCode.ToHashCode();
    }
}