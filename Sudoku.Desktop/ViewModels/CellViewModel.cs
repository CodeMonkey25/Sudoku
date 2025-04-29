using System;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Sudoku.Utility;

namespace Sudoku.ViewModels;

public partial class CellViewModel : ViewModelBase
{
    [Reactive] private Cell _cell = new(-1);
    [Reactive] private ICommand? _solveValueCommand = NullCommand.Instance;
    
    public string Value => Cell.Value == 0 ? " " : Cell.Value.ToString();
    public string Notes => Cell.IsSolved ? string.Empty : string.Join(" ", Cell.Candidates.Order());
    public bool IsValueVisible => Cell.IsSolved;
    public bool IsNotesVisible => !Cell.IsSolved;
    
    public event EventHandler IsSolvingEventHandler = delegate { };
    public event EventHandler IsSolvedEventHandler = delegate { };
    
    public CellViewModel()
    {
        SolveValueCommand = ReactiveCommand.Create<int, Unit>(SolveValue);
    }

    public void Tick()
    {
        this.RaisePropertyChanged(nameof(Value));
        this.RaisePropertyChanged(nameof(Notes));
        this.RaisePropertyChanged(nameof(IsValueVisible));
        this.RaisePropertyChanged(nameof(IsNotesVisible));
    }

    private Unit SolveValue(int value)
    {
        IsSolvingEventHandler(this, EventArgs.Empty);
        Cell.Solve(value);
        IsSolvedEventHandler(this, EventArgs.Empty);
        return Unit.Default;
    }
}