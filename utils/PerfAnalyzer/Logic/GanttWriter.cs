using System.Collections.Generic;
using System.IO;
using System.Linq;
using PerfAnalyzer.Models;

namespace PerfAnalyzer.Logic
{
    public class GanttWriter
    {
        public void Write(List<GanttEntry> ganttEntries, string outputFilePath)
        {
            var ganttLines = new[] { "Block\tStart [ms]\tDuration [ms]\tDescription" }.Concat(ganttEntries.Select(ToGanttLine))
                                                                                      .ToList();

            File.WriteAllLines(outputFilePath, ganttLines);
        }

        private string ToGanttLine(GanttEntry ganttEntry)
        {
            var description = $"{ganttEntry.BlockName}, {ganttEntry.DurationMsRoundedString}ms, DataId: {ganttEntry.DataId}";

            return $"{ganttEntry.BlockName}\t{ganttEntry.StartMsString}\t{ganttEntry.DurationMsString}\t{description}";
        }
    }
}
