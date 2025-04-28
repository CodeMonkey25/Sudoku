using System.Reactive.Disposables;
using ReactiveUI;

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

    protected virtual void HandleActivation(CompositeDisposable disposables) { }
    protected virtual void HandleDeactivation() { }
}