using System.Collections.Generic;
using System.IO;
using PerfAnalyzer.Models;

namespace PerfAnalyzer.Logic
{
    public class Gantt2Writer
    {
        public void Write(List<GanttEntry> ganttEntries, string outputFilePath)
        {
            File.WriteAllLines(outputFilePath, new[] { "test" });
        }
    }
}
