using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Sudoku.ViewModels;

namespace Sudoku.Views;

public partial class CellView : ReactiveUserControl<CellViewModel>
{
    public CellView()
    {
        InitializeComponent();
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
                .Select(i=> new MenuItem { Header = i.ToString(), Command = ViewModel.SolveValueCommand, CommandParameter = i})
                .ToArray(),
            Placement = PlacementMode.Bottom
        };
        FlyoutBase.SetAttachedFlyout(this, flyout);
        FlyoutBase.ShowAttachedFlyout(this);
    }
}