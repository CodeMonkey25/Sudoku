using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Sudoku.ViewModels;

namespace Sudoku.Views;

public partial class LibraryView : ReactiveUserControl<LibraryViewModel>
{
    public LibraryView()
    {
        InitializeComponent();
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ViewModel is null) return;
        ViewModel.LevelChanged();
    }
}