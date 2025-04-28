using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Splat;
using Sudoku.ViewModels;

namespace Sudoku.Views;

public partial class BoardView : ReactiveUserControl<BoardViewModel>
{
    public BoardView()
    {
        InitializeComponent();
        
        foreach (int i in Enumerable.Range(0, 81))
        {
            Border cellBorder = this.FindControl<Border>($"Border{i+1:00}") ?? throw new Exception($"Border{i+1:00} not found");
            CellView cellView = this.FindControl<CellView>($"Cell{i+1:00}") ?? throw new Exception($"Cell{i+1:00} not found");
            cellView.ViewModel = Locator.Current.GetService<CellViewModel>();
            cellView.ViewModel!.Cell = ViewModel!.Board.Cells[i];

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
            cellBorder.BorderBrush = Brushes.Black;
        }

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm =>
                {
                    foreach (int i in Enumerable.Range(0, 81))
                    {
                        CellView? cellView = this.FindControl<CellView>($"Cell{i+1:00}");
                        if (cellView is null) continue;
                        cellView.ViewModel!.Cell = vm!.Board.Cells[i];
                    }
                })
                .DisposeWith(disposables);
        });
    }

    public void LoadPuzzle(string puzzleText)
    {
        if (ViewModel is null) return;
        ViewModel.LoadPuzzle(puzzleText);
    }

    public string GetPuzzle()
    {
        if (ViewModel is null) return string.Empty;
        return ViewModel.GetPuzzle();
    }

    public void Tick()
    {
        foreach (int i in Enumerable.Range(0, 81))
        {
            CellView cellView = this.FindControl<CellView>($"Cell{i+1:00}") ?? throw new Exception($"Cell{i+1:00} not found");
            cellView.Tick();
        }
    }
}