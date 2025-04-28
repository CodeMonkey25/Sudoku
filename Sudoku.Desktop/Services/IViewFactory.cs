using Sudoku.ViewModels;

namespace Sudoku.Services;

public interface IViewFactory
{
     TViewModel CreateView<TViewModel>() where TViewModel : ViewModelBase;
}