using System;
using System.Diagnostics;
using Sudoku;
using Sudoku.Extensions;

namespace ConsoleClient
{
    internal static class Program
    {
        private static void Main()
        {
            // const string puzzleText = "4,,,,9,,,8,,,,,5,,,7,,,6,2,3,7,,,,4,,,4,9,,,,,7,3,,,,,,,,,,7,6,,,,,9,2,,,3,,,,2,4,1,5,,,2,,,6,,,,,1,,,5,,,,7";
            const string puzzleText = Puzzles.L2N122;
            int[] puzzle = LoadPuzzle(puzzleText);

            int[] solution = [];
            string timing = TimeIt(() => solution = Engine.Solve(puzzle));
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
            for (var i = 0; i < puzzle.Length; i++)
            {
                var num = puzzle[i];
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

        private static int[] LoadPuzzle(string puzzle)
        {
            int[] loadedPuzzle = Array.Empty<int>();

            if (!string.IsNullOrEmpty(puzzle))
            {
                if (puzzle.Contains(' '))
                    loadedPuzzle = LoadPuzzleWithSpaces(puzzle);
                
                if (puzzle.Contains(','))
                    loadedPuzzle = LoadPuzzleWithCommas(puzzle);
            }

            if (loadedPuzzle.Length != 81)
                throw new Exception("Puzzle is malformed: cell count is not 81");

            return loadedPuzzle;
        }

        private static int[] LoadPuzzleWithCommas(string puzzle)
        {
            // comma seperated format (4,,,,9,,,8 ...)

            int[] loadedPuzzle = new int[81];
            int i = 0;
            foreach (char c in puzzle)
            {
                if (i >= loadedPuzzle.Length) throw new Exception("Puzzle is malformed: cell count is not 81");

                switch (c)
                {
                    case ',':
                        i++;
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        loadedPuzzle[i] = c - '0';
                        break;
                }
            }
            if (i != (loadedPuzzle.Length - 1)) throw new Exception("Puzzle is malformed: cell count is not 81");

            return loadedPuzzle;
        }

        private static int[] LoadPuzzleWithSpaces(string puzzle)
        {
            // alternate format (530 070 000 ...)

            int j = 0;
            int[] loadedPuzzle = new int[81];
            for (var i = 0; i < puzzle.Length; i++)
            {
                var c = puzzle[i];
                if (char.IsDigit(c)) 
                    loadedPuzzle[j++] = c - '0';
            }
            if (j != loadedPuzzle.Length) throw new Exception("Puzzle is malformed: cell count is not 81");
            return loadedPuzzle;
        }
    }
}