using System;
using System.IO;
using System.Linq;
using CsvHelper;

namespace PerfAnalyzer
{
    class Program
    {
        private const string PERF_INPUT_PATH = @"..\..\..\..\perf_results\result.csv";
        private const string GANTT_OUTPUT_PATH = @"..\..\..\..\perf_results\gantt.csv";

        static void Main(string[] args)
        {
            var csvReader = GetCsvReader(PERF_INPUT_PATH);
            var perfEntries = csvReader.GetRecords<PerfEntry>().Where(x => x.EventName == "BlockExit").ToList();

            var blockDurations = perfEntries.GroupBy(x => x.BlockName)
                                            .ToDictionary(x => x.Key, x => x.Sum(item => item.ElapsedMs));

            foreach (var duration in blockDurations)
            {
                Console.WriteLine($"{duration.Key}: {duration.Value}ms");
            }

            var ganttEntries = perfEntries.Select(x => new GanttEntry
                {
                    BlockName = x.BlockName,
                    StartMs = x.Timestamp - x.ElapsedMs,
                    DurationMs = x.ElapsedMs,
                    DataId = "TODO"
                })
                                          .OrderBy(x => x.StartMs)
                                          .ToList();

            var ganttLines = new[] { "Task\tStart [ms]\tDuration [ms]\tDescription" }.Concat(ganttEntries.Select(x => x.ToChartLine())).ToList();
            File.WriteAllLines(GANTT_OUTPUT_PATH, ganttLines);
        }

        private static CsvReader GetCsvReader(string perfResultPath)
        {
            var textReader = File.OpenText(perfResultPath);

            var csvReader = new CsvReader(textReader);
            csvReader.Configuration.Delimiter = "\t";
            csvReader.Configuration.RegisterClassMap<PerfEntryMap>();

            return csvReader;
        }
    }
}
