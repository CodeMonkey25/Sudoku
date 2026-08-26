using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;

namespace Sudoku.ViewModels;

public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    public ViewModelBase()
    {
        this.WhenActivated(disposables => 
        {        
            HandleActivation(disposables);
            Disposable.Create(HandleDeactivation).DisposeWith(disposables);
        });
    }

    protected virtual void HandleActivation(MultipleDisposable disposables) { }
    protected virtual void HandleDeactivation() { }
}