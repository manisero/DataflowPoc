using System.Globalization;

namespace PerfAnalyzer.Extensions
{
    public static class DoubleExtensions
    {
        private static readonly NumberFormatInfo DoubleFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };

        public static string ToCsvString(this double value) => value.ToString(DoubleFormat);
    }
}
