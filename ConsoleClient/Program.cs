using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku;

namespace ConsoleClient
{
    internal static class Program
    {
        private static readonly string RowDivider = new('-', 19);
        
        private static void Main()
        {
            // const string puzzleText = "4,,,,9,,,8,,,,,5,,,7,,,6,2,3,7,,,,4,,,4,9,,,,,7,3,,,,,,,,,,7,6,,,,,9,2,,,3,,,,2,4,1,5,,,2,,,6,,,,,1,,,5,,,,7";
            const string puzzleText = Puzzles.L1N035;
            int[] puzzle = LoadPuzzle(puzzleText);

            int[] solution = Array.Empty<int>();
            string timing = Utility.TimeIt(() => solution = Engine.Solve(puzzle));
            PrintPuzzle(solution);

            Console.WriteLine(timing);
        }


        private static void PrintPuzzle(int[] puzzle)
        {
            Console.WriteLine(RowDivider);
            int cell = 1;
            foreach (int i in puzzle)
            {
                char c = (char)('0' + i);
                Console.Write('|');
                Console.Write(c == '0' ? ' ' : c);

                if (cell % 9 == 0)
                {
                    Console.WriteLine('|');
                    Console.WriteLine(RowDivider);
                }

                ++cell;
            }
        }

        private static int[] LoadPuzzle(string puzzle)
        {
            int[] loadedPuzzle = Array.Empty<int>();

            if (!string.IsNullOrEmpty(puzzle))
            {
                if (puzzle.Contains(','))
                    loadedPuzzle = LoadPuzzleWithCommas(puzzle);

                if (puzzle.Contains('0'))
                    loadedPuzzle = LoadPuzzleWithZeroes(puzzle);
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

            return loadedPuzzle;
        }

        private static int[] LoadPuzzleWithZeroes(IEnumerable<char> puzzle)
        {
            // alternate format (530 070 000 ...)
            return puzzle.Where(char.IsDigit).Select(c => c - '0').ToArray();
        }
    }
}