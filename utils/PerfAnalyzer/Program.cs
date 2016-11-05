using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using PerfAnalyzer.Models;

namespace PerfAnalyzer
{
    class Program
    {
        private const string PERF_INPUT_PATH = @"..\..\..\..\perf_results\result.csv";
        private const string GANTT_OUTPUT_PATH = @"..\..\..\..\perf_results\gantt.csv";

        static void Main(string[] args)
        {
            var csvReader = GetCsvReader(PERF_INPUT_PATH);
            var perfEntries = csvReader.GetRecords<PerfEntry>().OrderBy(x => x.TimestampMs).ToList();

            PrintGeneralResult(perfEntries);
            WriteGanttData(perfEntries);
        }

        private static CsvReader GetCsvReader(string perfResultPath)
        {
            var textReader = File.OpenText(perfResultPath);

            var csvReader = new CsvReader(textReader);
            csvReader.Configuration.Delimiter = "\t";
            csvReader.Configuration.RegisterClassMap<PerfEntryMap>();

            return csvReader;
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

        private static void WriteGanttData(List<PerfEntry> perfEntries)
        {
            var ganttEntries = perfEntries.GroupBy(x => new GanttEntry.Key
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
                                                  DurationMs = x.Exit.TimestampMs - x.Enter.TimestampMs
                                              })
                                          .OrderBy(x => x.StartMs)
                                          .ToList();

            var ganttLines = new[] { "Block\tStart [ms]\tDuration [ms]\tDescription" }.Concat(ganttEntries.Select(x => x.ToChartLine())).ToList();
            File.WriteAllLines(GANTT_OUTPUT_PATH, ganttLines);
        }
    }
}
