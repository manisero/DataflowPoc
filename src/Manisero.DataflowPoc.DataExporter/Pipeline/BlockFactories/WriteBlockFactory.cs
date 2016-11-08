using System;
using System.IO;
using System.Threading;
using CsvHelper;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Extensions;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories
{
    public interface IWriteBlockFactory
    {
        ProcessingBlock<DataBatch<TItem>> Create<TItem>(string targetFilePath, CancellationToken cancellation);
    }

    public class WriteBlockFactory : IWriteBlockFactory
    {
        public ProcessingBlock<DataBatch<TItem>> Create<TItem>(string targetFilePath, CancellationToken cancellation)
        {
            var csvWriter = new Lazy<CsvWriter>(() => CreateCsvWriter(targetFilePath));

            // Create blocks
            var writeBlock = DataflowFacade.TransformBlock<DataBatch<TItem>>(
                "WritePeopleBatch",
                x => x.Number,
                x => csvWriter.Value.WriteRecords(x.Data),
                cancellation);

            // Handle completion
            var completion = writeBlock.Completion.ContinueWithStatusPropagation(_ => csvWriter.ValueIfCreated()?.Dispose());

            return new ProcessingBlock<DataBatch<TItem>>
                {
                    Processor = writeBlock,
                    Completion = completion
                };
        }

        private CsvWriter CreateCsvWriter(string targetFilePath)
        {
            var targetWriter = new StreamWriter(targetFilePath, true);

            return new CsvWriter(targetWriter);
        }
    }
}
