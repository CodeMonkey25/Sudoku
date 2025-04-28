
using ReactiveUI.SourceGenerators;

namespace Sudoku.ViewModels;

public partial class LoadPuzzleWindowViewModel : ViewModelBase
{
    [Reactive] private string _puzzleText = """
                                            037 000 090
                                            050 930 012
                                            000 000 000
                                            000 100 900
                                            800 274 003
                                            003 005 000
                                            000 000 000
                                            370 062 080
                                            060 000 150
                                            """;

}