using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Splat;
using Sudoku.Extensions;
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

    public static readonly DirectProperty<MainWindow, ICommand> UndoCommandProperty = AvaloniaProperty.RegisterDirect<MainWindow, ICommand>(nameof(UndoCommand), o => o.UndoCommand, (o, v) => o.UndoCommand = v);
    private ICommand _undoCommand = NullCommand.Instance;
    public ICommand UndoCommand
    {
        get => _undoCommand;
        set => SetAndRaise(UndoCommandProperty, ref _undoCommand, value);
    }

    public static readonly DirectProperty<MainWindow, ICommand> SetGivenCommandProperty = AvaloniaProperty.RegisterDirect<MainWindow, ICommand>(nameof(SetGivenCommand), o => o.SetGivenCommand, (o, v) => o.SetGivenCommand = v);
    private ICommand _setGivenCommand = NullCommand.Instance;
    public ICommand SetGivenCommand
    {
        get => _setGivenCommand;
        set => SetAndRaise(SetGivenCommandProperty, ref _setGivenCommand, value);
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
        UndoCommand = ReactiveCommand.Create(Undo);
        SetGivenCommand = ReactiveCommand.Create(SetGiven);
        RunSolverCommand = ReactiveCommand.Create(RunSolver);
        
        Board.ViewModel = Locator.Current.GetService<BoardViewModel>();

        this.WhenActivated(disposables =>
        {
            Observable.Interval(TimeSpan.FromSeconds(0.5))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Board.Tick())
                .DisposeWith(disposables);
        });
    }

    private async Task LoadPuzzle()
    {
        if (ViewModel is null) return;

        string puzzle = """
                        037 000 090
                        050 930 012
                        000 000 000
                        000 100 900
                        800 274 003
                        003 005 000
                        000 000 000
                        370 062 080
                        060 000 150
                        """;
        string? result = await this.ShowInputBox("Please enter a puzzle:", puzzle);
        
        if (string.IsNullOrWhiteSpace(result)) return;
        Board.LoadPuzzle(result);
    }

    private void Exit()
    {
        Close();
    }

    private void Undo()
    {
        Board.Undo();
    }

    private void SetGiven()
    {
        Board.SetGiven();
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