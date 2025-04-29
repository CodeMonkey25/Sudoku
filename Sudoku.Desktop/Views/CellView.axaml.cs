using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
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

    public static readonly DirectProperty<CellView, IBrush?> ValueBrushProperty = AvaloniaProperty.RegisterDirect<CellView, IBrush?>(nameof(ValueBrush), o => o.ValueBrush, (o, v) => o.ValueBrush = v);
    private IBrush? _valueBrush;
    public IBrush? ValueBrush
    {
        get => _valueBrush;
        set => SetAndRaise(ValueBrushProperty, ref _valueBrush, value);
    }
    
    public CellView()
    {
        InitializeComponent();
        SolveCellCommand = ReactiveCommand.Create<int, Task<int>>(SolveCell);
        ValueBrush = this.Foreground;
    }

    public void Tick()
    {
        if (ViewModel is null) return;
        ValueBrush = ViewModel.Cell.IsGiven ? Brushes.Green : this.Foreground;
        ViewModel.Tick();
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
        if (TopLevel.GetTopLevel(this) is not MainWindow window) return 0;
        if (ViewModel?.SolveValueCommand is null) return 0;
        
        try
        {
            ViewModel.SolveValueCommand.Execute(value);
            if (window.IsInputGiven)
            {
                ViewModel.Cell.IsGiven = true;
            }
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