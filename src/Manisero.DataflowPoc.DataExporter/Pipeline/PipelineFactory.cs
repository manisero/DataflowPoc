using System;
using System.Threading;
using Manisero.DataflowPoc.Core.Pipelines;
using Manisero.DataflowPoc.Core.Pipelines.GenericBlockFactories;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Pipeline
{
    public interface IPipelineFactory
    {
        StartableBlock<DataBatch<Person>> Create(string targetFilePath, IProgress<PipelineProgress> progress, CancellationTokenSource cancellation);
    }

    public class PipelineFactory : IPipelineFactory
    {
        private readonly IReadBlockFactory _readBlockFactory;
        private readonly IWriteBlockFactory _writeBlockFactory;
        private readonly IProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly IStraightPipelineFactory _straightPipelineFactory;

        public PipelineFactory(IReadBlockFactory readBlockFactory,
                               IWriteBlockFactory writeBlockFactory,
                               IProgressReportingBlockFactory progressReportingBlockFactory,
                               IStraightPipelineFactory straightPipelineFactory)
        {
            _readBlockFactory = readBlockFactory;
            _writeBlockFactory = writeBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _straightPipelineFactory = straightPipelineFactory;
        }

        public StartableBlock<DataBatch<Person>> Create(string targetFilePath, IProgress<PipelineProgress> progress, CancellationTokenSource cancellation)
        {
            // TODO: Writing summary before people in the csv file

            // Create blocks
            var readBlock = _readBlockFactory.Create(cancellation.Token);
            var writeBlock = _writeBlockFactory.Create(targetFilePath, cancellation.Token);
            var progressBlock = _progressReportingBlockFactory.Create<DataBatch<Person>>("PeopleBatchProgress",
                                                                                         x => x.Number,
                                                                                         progress,
                                                                                         readBlock.EstimatedOutputCount,
                                                                                         1,
                                                                                         cancellation.Token);

            // Link blocks
            var pipeline = _straightPipelineFactory.Create(readBlock,
                                                           new[] { writeBlock, progressBlock },
                                                           cancellation);

            return pipeline;
        }
    }
}
