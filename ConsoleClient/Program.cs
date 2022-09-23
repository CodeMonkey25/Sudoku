using System;
using System.Linq;
using Sudoku;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] puzzle = LoadPuzzle("4,,,,9,,,8,,,,,5,,,7,,,6,2,3,7,,,,4,,,4,9,,,,,7,3,,,,,,,,,,7,6,,,,,9,2,,,3,,,,2,4,1,5,,,2,,,6,,,,,1,,,5,,,,7");
            PrintPuzzle(puzzle);
            Console.WriteLine();
            
            Engine engine = new();
            int[] solution = new int[81];
            Action action = () => solution = engine.Solve(puzzle);
            Console.WriteLine(Utility.TimeIt(action));
            PrintPuzzle(solution);
        }
        
        private static int[] LoadPuzzle(string puzzle)
        {
            string[] strings = puzzle.Split(',');
            if (strings.Length != 81) throw new Exception("Puzzle is malformed: cell count is not 81");
            if (strings.Any(s => s.Length > 1)) throw new Exception("Puzzle is malformed: all cell lengths are not <= 0");

            char[] chars = strings.Select(s => s == string.Empty ? '0' : s.First()).ToArray();
            if (!chars.All(char.IsDigit)) throw new Exception("Puzzle is malformed: all cells are not blank or a single digit");
            
            int[] ints = chars.Select(c=>c - '0').ToArray();

            return ints;
        }

        private static void PrintPuzzle(int[] puzzle)
        {
            string divider = new('-', 19);

            Console.WriteLine(divider);
            int cell = 1;
            foreach (char c in puzzle.Select(i => '0' + i))
            {
                Console.Write('|');
                Console.Write(c == '0' ? ' ' : c);
                
                if (cell % 9 == 0)
                {
                    Console.WriteLine('|');
                    Console.WriteLine(divider);
                }

                ++cell;
            }
        }
    }
}