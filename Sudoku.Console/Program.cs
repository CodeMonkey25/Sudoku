using System;
using System.Diagnostics;
using Sudoku.Extensions;

namespace Sudoku
{
    internal static class Program
    {
        private static void Main()
        {
            // const string puzzleText = "4,,,,9,,,8,,,,,5,,,7,,,6,2,3,7,,,,4,,,4,9,,,,,7,3,,,,,,,,,,7,6,,,,,9,2,,,3,,,,2,4,1,5,,,2,,,6,,,,,1,,,5,,,,7";
            const string puzzleText = Puzzles.L3N126;
            int[] puzzle = Board.ParsePuzzle(puzzleText);

            Engine engine = new(Console.WriteLine);
            int[] solution = [];
            string timing = TimeIt(() => solution = engine.Solve(puzzle));
            PrintPuzzle(solution);

            Console.WriteLine(timing);
        }

        private static string TimeIt(Action action, int count = 1)
        {
            Stopwatch watch = Stopwatch.StartNew();
            while (count > 0)
            {
                action.Invoke();
                --count;
            }

            watch.Stop();
            return $"Total time: {watch.Elapsed.ReadableTime()}";
        }

        private static void PrintPuzzle(int[] puzzle)
        {
            char[] rowDivider = new char[19];
            Array.Fill(rowDivider, '-');
            
            Console.WriteLine(rowDivider);
            int cell = 1;
            foreach (int num in puzzle)
            {
                char c = (char)('0' + num);
                Console.Write('|');
                Console.Write(c == '0' ? ' ' : c);

                if (cell % 9 == 0)
                {
                    Console.WriteLine('|');
                    Console.WriteLine(rowDivider);
                }

                ++cell;
            }
        }
    }
}