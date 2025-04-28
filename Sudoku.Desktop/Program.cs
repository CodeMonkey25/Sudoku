using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Splat;
using Sudoku.Services;
using Sudoku.ViewModels;

namespace Sudoku;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        RegisterDependencies();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    private static void RegisterDependencies()
    {
        SplatRegistrations.Register<IViewFactory, ViewFactory>();
        SplatRegistrations.Register<MainWindowViewModel>();
        SplatRegistrations.Register<SolverWindowViewModel>();
        SplatRegistrations.Register<BoardViewModel>();
        SplatRegistrations.Register<CellViewModel>();
        
        SplatRegistrations.SetupIOC();
    }
}