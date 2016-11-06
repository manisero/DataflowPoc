using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using PerfAnalyzer.Logic;
using PerfAnalyzer.Models;

namespace PerfAnalyzer
{
    class Program
    {
        private const string PERF_INPUT_PATH = @"..\..\..\..\perf_results\result.csv";
        private const string GANTT_OUTPUT_PATH = @"..\..\..\..\perf_results\gantt.csv";
        private const string GANTT2_OUTPUT_PATH = @"..\..\..\..\perf_results\gantt2.csv";

        static void Main(string[] args)
        {
            var perfEntries = ReadPerfEntries(PERF_INPUT_PATH);
            
            PrintGeneralResult(perfEntries);

            var ganttEntries = MapToGanttEntries(perfEntries);

            new GanttWriter().Write(ganttEntries, GANTT_OUTPUT_PATH);
            new Gantt2Writer().Write(ganttEntries, GANTT2_OUTPUT_PATH);
        }

        private static List<PerfEntry> ReadPerfEntries(string perfResultPath)
        {
            using (var textReader = File.OpenText(perfResultPath))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.Configuration.Delimiter = "\t";
                csvReader.Configuration.RegisterClassMap<PerfEntryMap>();

                return csvReader.GetRecords<PerfEntry>()
                                .OrderBy(x => x.TimestampMs)
                                .ToList();
            }
        }

        private static List<GanttEntry> MapToGanttEntries(IEnumerable<PerfEntry> perfEntries)
        {
            return perfEntries.GroupBy(x => new GanttEntry.Key
                {
                    BlockName = x.BlockName,
                    DataId = x.DataId
                })
                              .Select(x => new
                                  {
                                      x.Key,
                                      Enter = x.Single(entry => entry.EventName == EventNames.BLOCK_ENTER),
                                      Exit = x.Single(entry => entry.EventName == EventNames.BLOCK_EXIT)
                                  })
                              .Select(x => new GanttEntry
                                  {
                                      BlockName = x.Key.BlockName,
                                      DataId = x.Key.DataId,
                                      StartMs = x.Enter.TimestampMs,
                                      EndMs = x.Exit.TimestampMs
                                  })
                              .OrderBy(x => x.StartMs)
                              .ToList();
        }

        private static void PrintGeneralResult(List<PerfEntry> perfEntries)
        {
            var totalDurationMs = perfEntries.Last().TimestampMs - perfEntries.First().TimestampMs;
            Console.WriteLine($"Total duration: {totalDurationMs}ms");
            Console.WriteLine();

            var blockDurations = perfEntries.GroupBy(x => x.BlockName)
                                            .ToDictionary(x => x.Key,
                                                          x => x.Sum(item => item.ElapsedMs));

            foreach (var duration in blockDurations)
            {
                Console.WriteLine($"{duration.Key}: {duration.Value}ms");
            }
        }
    }
}
