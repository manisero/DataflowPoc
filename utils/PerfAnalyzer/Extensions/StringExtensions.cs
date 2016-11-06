using System.Collections.Generic;

namespace PerfAnalyzer.Extensions
{
    public static class StringExtensions
    {
        public static string JoinWith(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);
    }
}
