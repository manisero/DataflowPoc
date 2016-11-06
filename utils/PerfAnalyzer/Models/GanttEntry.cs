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
        public string StartMsString => StartMs.ToString(DoubleFormat);

        public double DurationMs { get; set; }
        public string DurationMsString => DurationMs.ToString(DoubleFormat);
        public string DurationMsRoundedString => Math.Round(DurationMs, 2).ToString(DoubleFormat);
    }
}
