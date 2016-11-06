using System.Collections.Generic;
using System.IO;
using System.Linq;
using PerfAnalyzer.Extensions;
using PerfAnalyzer.Models;

namespace PerfAnalyzer.Logic
{
    public class Gantt2Writer
    {
        private class LineModel
        {
            public string DataBatchId { get; set; }
            public ICollection<BlockData> BlocksData { get; set; }
        }

        private class BlockData
        {
            public string WaitingMs { get; set; }
            public string ProcessingMs { get; set; }
            public string Description { get; set; }
        }

        public void Write(List<GanttEntry> ganttEntries, string outputFilePath)
        {
            var blockHeaders = BlockNames.BlocksOrdered.SelectMany(x => new[] {"Waiting [ms]", $"{x} [ms]", "Description" });
            var headers = new[] { "Data batch id" }.Concat(blockHeaders).JoinWith("\t");

            var lineModels = ganttEntries.GroupBy(x => x.DataId)
                                         .Select(ToLineModel)
                                         .ToList();

            var lines = lineModels.Select(ToLine);
            var content = new[] {headers}.Concat(lines).ToList();

            File.WriteAllLines(outputFilePath, content);
        }

        private LineModel ToLineModel(IGrouping<int, GanttEntry> batchEntries)
        {
            var line = new LineModel
                {
                    DataBatchId = batchEntries.Key.ToString(),
                    BlocksData = new List<BlockData>()
                };

            var orderedBlocks = batchEntries.OrderBy(x => BlockNames.BlockToOrderMap[x.BlockName]);
            var previousBlockEnd = 0d;

            foreach (var block in orderedBlocks)
            {
                line.BlocksData.Add(new BlockData
                    {
                        WaitingMs = (block.StartMs - previousBlockEnd).ToCsvString(),
                        ProcessingMs = block.DurationMs.ToCsvString(),
                        Description = $"{block.BlockName}, {block.DurationMsRounded.ToCsvString()}ms"
                    });

                previousBlockEnd = block.EndMs;
            }
            
            return line;
        }

        private string ToLine(LineModel lineModel)
        {
            var blocksData = lineModel.BlocksData.SelectMany(x => new[] {x.WaitingMs, x.ProcessingMs, x.Description});

            return new[] { lineModel.DataBatchId }.Concat(blocksData).JoinWith("\t");
        }
    }
}
