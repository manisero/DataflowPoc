using CsvHelper.Configuration;

namespace PerfAnalyzer
{
    public class PerfEntry
    {
        public string EventName { get; set; }

        public double Timestamp { get; set; }

        public string BlockName { get; set; }

        public int ElapsedMs { get; set; }
    }

    public class PerfEntryMap : CsvClassMap<PerfEntry>
    {
        public PerfEntryMap()
        {
            Map(m => m.EventName).Index(0).ConvertUsing(x => ParseEventName(x.GetField<string>(0)));
            Map(m => m.Timestamp).Index(1).ConvertUsing(x => ParseTimestamp(x.GetField<string>(1)));
            Map(m => m.BlockName).Index(3).ConvertUsing(x => ParseBlockName(x.GetField<string>(3)));
            Map(m => m.ElapsedMs).Index(4).ConvertUsing(x => ParseElapsedTicks(x.GetField<string>(4)));
        }

        private string ParseEventName(string value)
        {
            return value.Trim().Replace("Manisero.DataflowPoc/", string.Empty);
        }

        private double ParseTimestamp(string value)
        {
            return double.Parse(value.Replace(" ", string.Empty).Replace(new string((char)160, 1), string.Empty));
        }

        private string ParseBlockName(string value)
        {
            return value.Trim();
        }

        private static int ParseElapsedTicks(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                       ? int.Parse(value.Replace(" ", string.Empty).Replace(new string((char)160, 1), string.Empty))
                       : -1;
        }
    }
}
