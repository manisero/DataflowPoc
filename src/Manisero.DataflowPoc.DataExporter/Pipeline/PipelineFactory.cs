using System.Threading;
using Manisero.DataflowPoc.Core.Pipelines;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Pipeline
{
    public interface IPipelineFactory
    {
        StartableBlock<DataBatch<Person>> Create(string targetFilePath, CancellationTokenSource cancellation);
    }

    public class PipelineFactory : IPipelineFactory
    {
        private readonly IReadBlockFactory _readBlockFactory;
        private readonly IWriteBlockFactory _writeBlockFactory;
        private readonly IStraightPipelineFactory _straightPipelineFactory;

        public PipelineFactory(IReadBlockFactory readBlockFactory,
                               IWriteBlockFactory writeBlockFactory,
                               IStraightPipelineFactory straightPipelineFactory)
        {
            _readBlockFactory = readBlockFactory;
            _writeBlockFactory = writeBlockFactory;
            _straightPipelineFactory = straightPipelineFactory;
        }

        public StartableBlock<DataBatch<Person>> Create(string targetFilePath, CancellationTokenSource cancellation)
        {
            // TODO: Writing summary before people in the csv file

            // Create blocks
            var readBlock = _readBlockFactory.Create(cancellation.Token);
            var writeBlock = _writeBlockFactory.Create(targetFilePath, cancellation.Token);

            // Link blocks
            var pipeline = _straightPipelineFactory.Create(readBlock,
                                                           new[] { writeBlock },
                                                           cancellation);

            return pipeline;
        }
    }
}
