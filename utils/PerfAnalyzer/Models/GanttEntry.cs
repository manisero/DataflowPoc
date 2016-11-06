using System;

namespace PerfAnalyzer.Models
{
    public class GanttEntry
    {
        public struct Key
        {
            public string BlockName { get; set; }
            public int DataId { get; set; }
        }

        public string BlockName { get; set; }

        public int DataId { get; set; }

        public double StartMs { get; set; }

        public double EndMs { get; set; }

        public double DurationMs => EndMs - StartMs;
        public double DurationMsRounded => Math.Round(DurationMs, 2);
    }
}
