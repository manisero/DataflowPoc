using System;
using System.IO;
using System.Threading;
using Manisero.DataflowPoc.Core.Extensions;
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
        StartableBlock<DataBatch<Person>> Create(string targetFilePath, IProgress<PipelineProgress> progress, CancellationToken cancellation);
    }

    public class PipelineFactory : IPipelineFactory
    {
        private readonly IReadSummaryBlockFactory _readSummaryBlockFactory;
        private readonly IReadPeopleBlockFactory _readPeopleBlockFactory;
        private readonly IWriteCsvBlockFactory _writeCsvBlockFactory;
        private readonly IProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly IStraightPipelineFactory _straightPipelineFactory;

        public PipelineFactory(IReadSummaryBlockFactory readSummaryBlockFactory,
                               IReadPeopleBlockFactory readPeopleBlockFactory,
                               IWriteCsvBlockFactory writeCsvBlockFactory,
                               IProgressReportingBlockFactory progressReportingBlockFactory,
                               IStraightPipelineFactory straightPipelineFactory)
        {
            _readSummaryBlockFactory = readSummaryBlockFactory;
            _readPeopleBlockFactory = readPeopleBlockFactory;
            _writeCsvBlockFactory = writeCsvBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _straightPipelineFactory = straightPipelineFactory;
        }

        public StartableBlock<DataBatch<Person>> Create(string targetFilePath, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            File.Create(targetFilePath).Dispose();

            // Create pipelines
            var summaryPipeline = CreateSummaryPipeline(targetFilePath, progress, cancellation);

            var writeEmptyLineBlock = new StartableBlock<object>(
                () =>
                    {
                        if (!summaryPipeline.Completion.IsFaulted && !summaryPipeline.Completion.IsCanceled)
                        {
                            File.AppendAllLines(targetFilePath, new[] { string.Empty });
                        }
                    });

            var peoplePipeline = CreatePeoplePipeline(targetFilePath, progress, cancellation);

            // Link pipelines
            summaryPipeline.ContinueWith(writeEmptyLineBlock);
            writeEmptyLineBlock.ContinueWith(peoplePipeline);

            return new StartableBlock<DataBatch<Person>>
                {
                    Start = summaryPipeline.Start,
                    Output = peoplePipeline.Output,
                    EstimatedOutputCount = peoplePipeline.EstimatedOutputCount,
                    Completion = peoplePipeline.Completion
                };
        }

        private StartableBlock<DataBatch<PeopleSummary>> CreateSummaryPipeline(string targetFilePath, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            // Create blocks
            var readBlock = _readSummaryBlockFactory.Create(cancellationSource.Token);
            var writeBlock = _writeCsvBlockFactory.Create<PeopleSummary>(targetFilePath, true, cancellationSource.Token);
            var progressBlock = _progressReportingBlockFactory.Create("PeopleSummaryProgress",
                                                                      DataBatch<PeopleSummary>.IdGetter,
                                                                      progress,
                                                                      readBlock.EstimatedOutputCount,
                                                                      1,
                                                                      cancellationSource.Token);

            // Create pipeline
            var pipeline = _straightPipelineFactory.Create(readBlock,
                                                           new[] { writeBlock, progressBlock },
                                                           cancellationSource);

            pipeline.ContinueCompletionWith(_ => cancellationSource.Dispose());

            return pipeline;
        }

        private StartableBlock<DataBatch<Person>> CreatePeoplePipeline(string targetFilePath, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            // Create blocks
            var readBlock = _readPeopleBlockFactory.Create(cancellationSource.Token);
            var writeBlock = _writeCsvBlockFactory.Create<Person>(targetFilePath, true, cancellationSource.Token);
            var progressBlock = _progressReportingBlockFactory.Create("PersonProgress",
                                                                      DataBatch<Person>.IdGetter,
                                                                      progress,
                                                                      readBlock.EstimatedOutputCount,
                                                                      1,
                                                                      cancellationSource.Token);

            // Create pipeline
            var pipeline = _straightPipelineFactory.Create(readBlock,
                                                           new[] { writeBlock, progressBlock },
                                                           cancellationSource);

            pipeline.ContinueCompletionWith(_ => cancellationSource.Dispose());

            return pipeline;
        }
    }
}
