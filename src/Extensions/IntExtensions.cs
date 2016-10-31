using System;

namespace Dataflow.Extensions
{
    public static class IntExtensions
    {
        public static int CeilingOfDivisionBy(this int number, int dividor)
        {
            return (int)Math.Ceiling((decimal)number / dividor);
        }
    }
}
