using System.IO;
using System.Threading;
using CsvHelper;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Logic;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories
{
    public interface IWriteBlockFactory
    {
        ProcessingBlock<DataBatch<Person>> Create(string targetFilePath, CancellationToken cancellation);
    }

    public class WriteBlockFactory : IWriteBlockFactory
    {
        private readonly IPeopleBatchWriter _peopleBatchWriter;

        public WriteBlockFactory(IPeopleBatchWriter peopleBatchWriter)
        {
            _peopleBatchWriter = peopleBatchWriter;
        }

        public ProcessingBlock<DataBatch<Person>> Create(string targetFilePath, CancellationToken cancellation)
        {
            var targetWriter = new StreamWriter(targetFilePath);
            var csvWriter = new CsvWriter(targetWriter);

            // Create blocks
            var writeBlock = DataflowFacade.TransformBlock<DataBatch<Person>>(
                "WritePeopleBatch",
                x => x.Number,
                x => _peopleBatchWriter.Write(x, csvWriter),
                cancellation);

            // Handle completion
            var completion = writeBlock.Completion.ContinueWithStatusPropagation(_ => csvWriter.Dispose());

            return new ProcessingBlock<DataBatch<Person>>
                {
                    Processor = writeBlock,
                    Completion = completion
                };
        }
    }
}
