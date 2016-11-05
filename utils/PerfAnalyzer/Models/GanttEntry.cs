using System;
using System.Globalization;

namespace PerfAnalyzer.Models
{
    public class GanttEntry
    {
        public struct Key
        {
            public string BlockName { get; set; }

            public int DataId { get; set; }
        }

        private static readonly NumberFormatInfo DoubleFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };

        public string BlockName { get; set; }

        public int DataId { get; set; }

        public double StartMs { get; set; }

        public double DurationMs { get; set; }

        public string Description => $"{BlockName}, {Math.Round(DurationMs, 2).ToString(DoubleFormat)}ms, DataId: {DataId}";

        public string ToChartLine() => $"{BlockName}\t{StartMs.ToString(DoubleFormat)}\t{DurationMs.ToString(DoubleFormat)}\t{Description}";
    }
}
