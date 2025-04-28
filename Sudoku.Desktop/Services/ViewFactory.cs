using System;
using Splat;
using Sudoku.ViewModels;

namespace Sudoku.Services;

public class ViewFactory : IViewFactory
{
    public TViewModel CreateView<TViewModel>() where TViewModel : ViewModelBase
    {
        return Locator.Current.GetService<TViewModel>() ?? throw new ArgumentOutOfRangeException(nameof(TViewModel), typeof(TViewModel).Name, null);
    }
}