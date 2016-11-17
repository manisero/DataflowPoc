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
    public interface IWriteCsvBlockFactory
    {
        ProcessingBlock<DataBatch<TItem>> Create<TItem>(string targetFilePath, bool appendTargetFile, CancellationToken cancellation);
    }

    public class WriteCsvBlockFactory : IWriteCsvBlockFactory
    {
        public ProcessingBlock<DataBatch<TItem>> Create<TItem>(string targetFilePath, bool appendTargetFile, CancellationToken cancellation)
        {
            var csvWriter = new Lazy<CsvWriter>(() => CreateCsvWriter(targetFilePath, appendTargetFile));

            // Create blocks
            var writeBlock = DataflowFacade.TransformBlock($"Write{typeof(TItem).Name}",
                                                           DataBatch<TItem>.IdGetter,
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

        private CsvWriter CreateCsvWriter(string targetFilePath, bool appendTargetFile)
        {
            var targetWriter = new StreamWriter(targetFilePath, appendTargetFile);

            return new CsvWriter(targetWriter);
        }
    }
}
