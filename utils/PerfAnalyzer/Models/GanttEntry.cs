using System.Globalization;

namespace PerfAnalyzer.Models
{
    public class GanttEntry
    {
        private static readonly NumberFormatInfo DoubleFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };

        public string BlockName { get; set; }

        public double StartMs { get; set; }

        public int DurationMs { get; set; }

        public string DataId { get; set; }

        public string Description => $"{BlockName}, {DurationMs}ms, DataId: {DataId}";

        public string ToChartLine() => $"{BlockName}\t{StartMs.ToString(DoubleFormat)}\t{DurationMs}\t{Description}";
    }
}
