using System;
using System.Diagnostics;
using System.Linq;
using Sudoku.Extensions;

namespace Sudoku
{
    public class Utility
    {
        public static string TimeIt(Action action, int count = 1)
        {
            Stopwatch watch = Stopwatch.StartNew();
            foreach (int i in Enumerable.Range(0, count))
                action.Invoke();
            watch.Stop();
            return $"Total time: {watch.Elapsed.ReadableTime()}";
        }
    }
}