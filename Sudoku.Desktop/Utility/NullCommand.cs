using System;
using System.Windows.Input;

namespace Sudoku.Utility;

public class NullCommand : ICommand
{
    public static ICommand Instance { get; } = new NullCommand();
    
    #region Interface
    private EventHandler? _canExecuteChanged;
    event EventHandler? ICommand.CanExecuteChanged
    {
        add => _canExecuteChanged += value;
        remove => _canExecuteChanged -= value;
    }

    protected void OnCanExecuteChanged()
    {
        _canExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    
    bool ICommand.CanExecute(object? parameter) { return false; }
    void ICommand.Execute(object? parameter) { }
    #endregion

    private NullCommand() { }
}