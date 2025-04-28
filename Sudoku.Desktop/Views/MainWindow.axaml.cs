using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Splat;
using Sudoku.Services;
using Sudoku.Utility;
using Sudoku.ViewModels;

namespace Sudoku.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public static readonly DirectProperty<MainWindow, ICommand> LoadCommandProperty = AvaloniaProperty.RegisterDirect<MainWindow, ICommand>(nameof(LoadPuzzleCommand), static o => o.LoadPuzzleCommand, static (o, v) => o.LoadPuzzleCommand = v);
    private ICommand _loadPuzzleCommand = NullCommand.Instance;
    public ICommand LoadPuzzleCommand
    {
        get => _loadPuzzleCommand;
        set => SetAndRaise(LoadCommandProperty, ref _loadPuzzleCommand, value);
    }
    
    public static readonly DirectProperty<MainWindow, ICommand> ExitCommandProperty = AvaloniaProperty.RegisterDirect<MainWindow, ICommand>(nameof(ExitCommand), static o => o.ExitCommand, static (o, v) => o.ExitCommand = v);
    private ICommand _exitCommand = NullCommand.Instance;
    public ICommand ExitCommand
    {
        get => _exitCommand;
        set => SetAndRaise(ExitCommandProperty, ref _exitCommand, value);
    }
    
    public static readonly DirectProperty<MainWindow, ICommand> RunSolverProperty = AvaloniaProperty.RegisterDirect<MainWindow, ICommand>(nameof(RunSolverCommand), static o => o.RunSolverCommand, static (o, v) => o.RunSolverCommand = v);
    private ICommand _runSolverCommand = NullCommand.Instance;
    public ICommand RunSolverCommand
    {
        get => _runSolverCommand;
        set => SetAndRaise(RunSolverProperty, ref _runSolverCommand, value);
    }
    
    public MainWindow()
    {
        InitializeComponent();
        
        LoadPuzzleCommand = ReactiveCommand.Create(LoadPuzzle);
        ExitCommand = ReactiveCommand.Create(Exit);
        RunSolverCommand = ReactiveCommand.Create(RunSolver);
        
        // Board.ViewModel = Locator.Current.GetService<BoardViewModel>();

        this.WhenActivated(disposables =>
        {
            Observable.Interval(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Board.Tick())
                .DisposeWith(disposables);
        });
    }
    
    private async Task LoadPuzzle()
    {
        if (ViewModel is null) return;
        IViewFactory? viewFactory = Locator.Current.GetService<IViewFactory>();
        if (viewFactory is null) return;
        
        LoadPuzzleWindow dialog = new() { DataContext = viewFactory.CreateView<LoadPuzzleWindowViewModel>() };
        string? result = await dialog.ShowDialog<string?>(this);
        if (string.IsNullOrWhiteSpace(result)) return;
        
        Board.LoadPuzzle(result);
    }

    private void Exit()
    {
        Close();
    }

    private async Task RunSolver()
    {
        if (ViewModel is null) return;
        IViewFactory? viewFactory = Locator.Current.GetService<IViewFactory>();
        if (viewFactory is null) return;
        
        SolverWindow dialog = new() { DataContext = viewFactory.CreateView<SolverWindowViewModel>() };
        dialog.ViewModel?.SolvePuzzle(Board.GetPuzzle());
        string? result = await dialog.ShowDialog<string?>(this);
        
        // vm.LoadPuzzle(result);
    }
}