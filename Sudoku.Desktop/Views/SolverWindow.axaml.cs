using Avalonia.ReactiveUI;
using Sudoku.ViewModels;

namespace Sudoku.Views;

public partial class SolverWindow : ReactiveWindow<SolverWindowViewModel>
{
    public SolverWindow()
    {
        InitializeComponent();
    }
}