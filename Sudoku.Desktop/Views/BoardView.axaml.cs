using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Splat;
using Sudoku.ViewModels;

namespace Sudoku.Views;

public partial class BoardView : ReactiveUserControl<BoardViewModel>
{
    private readonly CellView[] _cellViews = new CellView[81];
    
    public BoardView()
    {
        InitializeComponent();
        
        foreach (int i in Enumerable.Range(0, 81))
        {
            Border cellBorder = this.FindControl<Border>($"Border{i+1:00}") ?? throw new Exception($"Border{i+1:00} not found");
            _cellViews[i] = this.FindControl<CellView>($"Cell{i+1:00}") ?? throw new Exception($"Cell{i+1:00} not found");
            _cellViews[i].ViewModel = Locator.Current.GetService<CellViewModel>();
            _cellViews[i].ViewModel!.Cell = ViewModel!.Board.Cells[i];
            
            double left = 0, top = 0, right = 0, bottom = 0;
            
            // add the single border around each cell 
            if (i % 9 == 0) left = 1;
            if(i < 9) top = 2;
            bottom = 1;
            right = 1;
            
            // double the thickness around each 3x3 grid
            if (i % 3 == 0) left = 2;
            if (i % 9 == 8) right = 2; 
            if((i / 9) % 3 == 2) bottom = 2;
            
            cellBorder.BorderThickness = new Thickness(left, top, right, bottom);
            cellBorder[!Border.BorderBrushProperty] = this[!UserControl.ForegroundProperty];
        }

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm =>
                {
                    foreach (CellView cellView in _cellViews)
                    {
                        ViewModel!.StoreCellViewModel(cellView.ViewModel!);
                        int i = cellView.ViewModel!.Cell.Index;
                        cellView.ViewModel!.Cell = vm!.Board.Cells[i];
                    }
                })
                .DisposeWith(disposables);
            
            Disposable.Create(() => Array.Fill(_cellViews, null)).DisposeWith(disposables);
        });
    }

    public void LoadPuzzle(string puzzleText)
    {
        ViewModel?.LoadPuzzle(puzzleText);
    }

    public void LoadPuzzle(string level, string number)
    {
        string puzzleText = (string?) typeof(Puzzles).GetField($"L{level}N{number}")?.GetValue(null) ?? string.Empty;
        ViewModel?.LoadPuzzle(puzzleText);
    }

    public string GetPuzzle()
    {
        return ViewModel?.GetPuzzle() ?? string.Empty;
    }

    public void Tick()
    {
        if (ViewModel is null) return;
        if (!ViewModel.IsDirty) return;

        ViewModel.Tick();
        foreach (CellView cellView in _cellViews)
        {
            cellView.Tick();
        }

        ViewModel.IsDirty = false;
    }

    public void Undo()
    {
        if (ViewModel is null) return;
        ViewModel.Undo();
    }
}