using Avalonia;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Sudoku.ViewModels;

namespace Sudoku.Views;

public partial class LoadPuzzleWindow : ReactiveWindow<LoadPuzzleWindowViewModel>
{
    public LoadPuzzleWindow()
    {
        InitializeComponent();
    }

    private void PuzzleTextBox_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        PuzzleTextBox.Focus();
        PuzzleTextBox.SelectAll();
    }

    private void OpenButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(ViewModel?.PuzzleText);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}