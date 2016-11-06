using System.Collections.Generic;
using System.Linq;

namespace PerfAnalyzer.Models
{
    public static class BlockNames
    {
        public const string ReadData = nameof(ReadData);
        public const string Validate = nameof(Validate);
        public const string ComputeFields = nameof(ComputeFields);
        public const string WriteData = nameof(WriteData);
        public const string ReportProgress = nameof(ReportProgress);

        public static readonly List<string> BlocksOrdered = new List<string>
            {
                ReadData,
                Validate,
                ComputeFields,
                WriteData,
                ReportProgress
            };

        public static readonly Dictionary<string, int> BlockToOrderMap = BlocksOrdered.Select((x, i) => new { Block = x, Order = i })
                                                                                      .ToDictionary(x => x.Block,
                                                                                                    x => x.Order);
    }
}
