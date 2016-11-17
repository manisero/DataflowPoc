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
        StartableBlock<DataBatch<Person>> Create(string targetFilePath, IProgress<PipelineProgress> progress, CancellationTokenSource cancellation);
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

        public StartableBlock<DataBatch<Person>> Create(string targetFilePath, IProgress<PipelineProgress> progress, CancellationTokenSource cancellation)
        {
            File.Create(targetFilePath).Dispose();

            // Create pipelines
            var summaryPipeline = CreateSummaryPipeline(targetFilePath, progress, cancellation);
            var peoplePipeline = CreatePeoplePipeline(targetFilePath, progress, cancellation);

            // TODO: New line between summary and people

            // Link pipelines
            summaryPipeline.ContinueWith(peoplePipeline);

            return new StartableBlock<DataBatch<Person>>
                {
                    Start = summaryPipeline.Start,
                    Output = peoplePipeline.Output,
                    EstimatedOutputCount = peoplePipeline.EstimatedOutputCount,
                    Completion = peoplePipeline.Completion
                };
        }

        private StartableBlock<DataBatch<PeopleSummary>> CreateSummaryPipeline(string targetFilePath, IProgress<PipelineProgress> progress, CancellationTokenSource cancellation)
        {
            var readBlock = _readSummaryBlockFactory.Create(cancellation.Token);
            var writeBlock = _writeCsvBlockFactory.Create<PeopleSummary>(targetFilePath, true, cancellation.Token);
            var progressBlock = _progressReportingBlockFactory.Create("PeopleSummaryProgress",
                                                                      DataBatch<PeopleSummary>.IdGetter,
                                                                      progress,
                                                                      readBlock.EstimatedOutputCount,
                                                                      1,
                                                                      cancellation.Token);

            return _straightPipelineFactory.Create(readBlock,
                                                   new[] { writeBlock, progressBlock },
                                                   cancellation);
        }

        private StartableBlock<DataBatch<Person>> CreatePeoplePipeline(string targetFilePath, IProgress<PipelineProgress> progress, CancellationTokenSource cancellation)
        {
            var readBlock = _readPeopleBlockFactory.Create(cancellation.Token);
            var writeBlock = _writeCsvBlockFactory.Create<Person>(targetFilePath, true, cancellation.Token);
            var progressBlock = _progressReportingBlockFactory.Create("PersonProgress",
                                                                      DataBatch<Person>.IdGetter,
                                                                      progress,
                                                                      readBlock.EstimatedOutputCount,
                                                                      1,
                                                                      cancellation.Token);

            return _straightPipelineFactory.Create(readBlock,
                                                   new[] { writeBlock, progressBlock },
                                                   cancellation);
        }
    }
}
