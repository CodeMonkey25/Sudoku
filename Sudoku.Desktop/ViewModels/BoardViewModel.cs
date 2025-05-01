using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace Sudoku.ViewModels;

public class BoardViewModel : ViewModelBase
{
    private CellViewModel?[] CellViewModels { get; } = new CellViewModel[81];
    private Stack<CellState[]> UndoStack { get; } = new();
    
    public Board Board { get; } = new();
    public bool IsDirty { get; set; } = true;
    
    protected override void HandleActivation(CompositeDisposable disposables)
    {
        base.HandleActivation(disposables);
        
        foreach (CellViewModel? cellViewModel in CellViewModels)
        {
            if (cellViewModel is null) continue;
            
            cellViewModel.IsSolvingEventHandler += CellIsSolving;
            cellViewModel.IsSolvedEventHandler += CellIsSolved;
        }
    }

    protected override void HandleDeactivation()
    {
        base.HandleDeactivation();
        
        foreach (CellViewModel? cellViewModel in CellViewModels)
        {
            if (cellViewModel is null) continue;
            
            cellViewModel.IsSolvingEventHandler -= CellIsSolving;
            cellViewModel.IsSolvedEventHandler -= CellIsSolved;
        }
        
        Array.Fill(CellViewModels, null);
    }
    
    public void LoadPuzzle(string puzzleText)
    {
        int[] puzzle = Board.ParsePuzzle(puzzleText);
        Board.LoadPuzzle(puzzle);
        IsDirty = true;
    }

    public string GetPuzzle()
    {
        return Board.GetOriginalPuzzle();
    }
    
    public void StoreCellViewModel(CellViewModel cellViewModel)
    {
        if (CellViewModels[cellViewModel.Cell.Index] is not null)
        {
            CellViewModels[cellViewModel.Cell.Index]!.IsSolvingEventHandler -= CellIsSolving;
            CellViewModels[cellViewModel.Cell.Index]!.IsSolvedEventHandler -= CellIsSolved;
        }
        CellViewModels[cellViewModel.Cell.Index] = cellViewModel;
        cellViewModel.IsSolvingEventHandler += CellIsSolving;
        cellViewModel.IsSolvedEventHandler += CellIsSolved;
    }

    private void CellIsSolving(object? sender, EventArgs e)
    {
        UndoStack.Push(Board.GetState());
    }

    private void CellIsSolved(object? sender, EventArgs e)
    {
        IsDirty = true;
    }

    public void Tick()
    {
        
    }

    public void Undo()
    {
        if (UndoStack.Count == 0) return;
        Board.RestoreState(UndoStack.Pop());
        IsDirty = true;
    }
}