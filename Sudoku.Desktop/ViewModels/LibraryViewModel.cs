using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using DynamicData;
using ReactiveUI.SourceGenerators;

namespace Sudoku.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
    [Reactive] private ObservableCollection<string> _levels = new();
    [Reactive] private ObservableCollection<string> _numbers = new();
    
    [Reactive] private string _level = string.Empty;
    [Reactive] private string _number = string.Empty;

    private Dictionary<string, List<string>> Puzzles { get; set; }

    public LibraryViewModel()
    {
        Puzzles = typeof(Puzzles)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
            .Select(fi => fi.Name)
            .GroupBy(n => n.Substring(1,1))
            .ToDictionary(
                g => g.Key,
                g => g.Select(n => n[3..]).ToList()
            );
        
        if (Puzzles.Count == 0) return;
        Levels.Add(Puzzles.Keys.Order());
        Level = Levels.First();
    }

    public void LevelChanged()
    {
        Numbers.Clear();
        Numbers.Add(Puzzles[Level].Order());
        if (Numbers.Count == 0) return;
        Number = Numbers.First();
    }
}