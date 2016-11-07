using System;
using System.Collections.Generic;

namespace Manisero.DataflowPoc.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
    }
}
