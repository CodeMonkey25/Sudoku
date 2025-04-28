using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using ReactiveUI;
using Sudoku.Extensions;
using Sudoku.Utility;
using Sudoku.ViewModels;

namespace Sudoku.Views;

public partial class CellView : ReactiveUserControl<CellViewModel>
{
    public static readonly DirectProperty<CellView, ICommand> SolveCellCommandProperty = AvaloniaProperty.RegisterDirect<CellView, ICommand>(nameof(SolveCellCommand), o => o.SolveCellCommand, (o, v) => o.SolveCellCommand = v);
    private ICommand _solveCellCommand = NullCommand.Instance;
    public ICommand SolveCellCommand
    {
        get => _solveCellCommand;
        set => SetAndRaise(SolveCellCommandProperty, ref _solveCellCommand, value);
    }
    
    public CellView()
    {
        InitializeComponent();
        SolveCellCommand = ReactiveCommand.Create<int, Task<int>>(SolveCell);
    }

    public void Tick()
    {
        ViewModel?.Tick();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel is null) return;
        
        MenuFlyout flyout = new()
        {
            ItemsSource = ViewModel
                .Cell
                .Candidates
                .Order()
                .Select(i=> new MenuItem { Header = i.ToString(), Command = SolveCellCommand, CommandParameter = i})
                .ToArray(),
            Placement = PlacementMode.Bottom
        };
        FlyoutBase.SetAttachedFlyout(this, flyout);
        FlyoutBase.ShowAttachedFlyout(this);
    }

    private async Task<int> SolveCell(int value)
    {
        try
        {
            ViewModel?.SolveValueCommand?.Execute(value);
        }
        catch
        {
            await this.ShowMessageBox("The puzzle is not solvable.\nPlease undo your last move and try again.");
            
            BoardView? boardView = this.GetSelfAndVisualAncestors().OfType<BoardView>().FirstOrDefault();
            if (boardView is { ViewModel: not null }) boardView.ViewModel.IsDirty = true;
        }

        return 0;
    }
}