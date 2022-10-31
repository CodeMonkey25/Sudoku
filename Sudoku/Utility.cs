using System;
using System.Diagnostics;
using Sudoku.Extensions;

namespace Sudoku
{
    public class Utility
    {
        public static string TimeIt(Action action, int count = 1)
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
    }
}