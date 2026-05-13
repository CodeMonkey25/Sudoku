using ReactiveUI.Avalonia;
using Sudoku.ViewModels;

namespace Sudoku.Views;

public partial class SolverWindow : ReactiveWindow<SolverWindowViewModel>
{
    public SolverWindow()
    {
        InitializeComponent();
    }
}