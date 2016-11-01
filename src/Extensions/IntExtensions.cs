using System;

namespace Dataflow.Extensions
{
    public static class IntExtensions
    {
        public static int CeilingOfDivisionBy(this int number, int dividor)
        {
            return (int)Math.Ceiling((decimal)number / dividor);
        }

        public static byte PercentageOf(this int number, int total)
        {
            return (byte)(number * 100 / total);
        }
    }
}
