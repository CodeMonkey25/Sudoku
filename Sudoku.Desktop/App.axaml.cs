using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Sudoku.Services;
using Sudoku.ViewModels;
using Sudoku.Views;
using Splat;

namespace Sudoku;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Locator.Current.GetService<IViewFactory>()?.CreateView<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}