using System.Collections.Generic;
using System.IO;
using System.Linq;
using PerfAnalyzer.Extensions;
using PerfAnalyzer.Models;

namespace PerfAnalyzer.Logic
{
    public class GanttWriter
    {
        public void Write(List<GanttEntry> ganttEntries, string outputFilePath)
        {
            const string header = "Block\tStart [ms]\tDuration [ms]\tDescription";

            var content = new[] { header }.Concat(ganttEntries.Select(ToGanttLine)).ToList();

            File.WriteAllLines(outputFilePath, content);
        }

        private string ToGanttLine(GanttEntry ganttEntry)
        {
            var description = $"{ganttEntry.BlockName}, {ganttEntry.DurationMsRounded.ToCsvString()}ms, DataId: {ganttEntry.DataId}";

            return $"{ganttEntry.BlockName}\t{ganttEntry.StartMs.ToCsvString()}\t{ganttEntry.DurationMs.ToCsvString()}\t{description}";
        }
    }
}
