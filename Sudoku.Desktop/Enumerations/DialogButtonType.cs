using System;

namespace Sudoku.Enumerations;

[Flags]
public enum DialogButtonType
{
    None = 0,
    Ok = 1 << 0,
    Cancel = 1 << 1,
}
