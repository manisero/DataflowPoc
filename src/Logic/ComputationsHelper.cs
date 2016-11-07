using System;

namespace Manisero.DataflowPoc.Logic
{
    public static class ComputationsHelper
    {
        public static void PerformTimeConsumingOperation()
        {
            for (var i = 0; i < 1000; i++)
            {
                var _ = Math.Pow(i, i);
            }
        }
    }
}
