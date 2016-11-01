using System;
using System.IO;
using System.Linq;
using CsvHelper;

namespace PerfAnalyzer
{
    class Program
    {
        private const string PERF_RESULT_PATH = @"..\..\..\..\perf_results\result.csv";

        static void Main(string[] args)
        {
            var csvReader = GetCsvReader(PERF_RESULT_PATH);
            var entries = csvReader.GetRecords<PerfEntry>().ToList();

            var blockDurations = entries.Where(x => x.EventName == "BlockExit")
                                        .GroupBy(x => x.BlockName)
                                        .ToDictionary(x => x.Key, x => x.Sum(item => item.ElapsedTicks));

            foreach (var duration in blockDurations)
            {
                Console.WriteLine($"{duration.Key}: {duration.Value} ticks");
            }
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
