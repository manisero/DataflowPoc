using System;
using System.IO;
using System.Linq;
using System.Threading;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines;
using Manisero.DataflowPoc.Core.Pipelines.GenericBlockFactories;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Logic;
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
        private readonly IPeopleSummaryBuilder _peopleSummaryBuilder;
        private readonly IWriteCsvBlockFactory _writeCsvBlockFactory;
        private readonly IProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly IStraightPipelineFactory _straightPipelineFactory;

        public PipelineFactory(IReadSummaryBlockFactory readSummaryBlockFactory,
                               IReadPeopleBlockFactory readPeopleBlockFactory,
                               IPeopleSummaryBuilder peopleSummaryBuilder,
                               IWriteCsvBlockFactory writeCsvBlockFactory,
                               IProgressReportingBlockFactory progressReportingBlockFactory,
                               IStraightPipelineFactory straightPipelineFactory)
        {
            _readSummaryBlockFactory = readSummaryBlockFactory;
            _readPeopleBlockFactory = readPeopleBlockFactory;
            _peopleSummaryBuilder = peopleSummaryBuilder;
            _writeCsvBlockFactory = writeCsvBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _straightPipelineFactory = straightPipelineFactory;
        }

        public StartableBlock<DataBatch<Person>> Create(string targetFilePath, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            File.Create(targetFilePath).Dispose();
            var aggregatedSummary = new PeopleSummary();

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

            var peoplePipeline = CreatePeoplePipeline(targetFilePath, aggregatedSummary, progress, cancellation);

            // Link pipelines
            summaryPipeline.ContinueWith(writeEmptyLineBlock);
            writeEmptyLineBlock.ContinueWith(peoplePipeline);

            return new StartableBlock<DataBatch<Person>>(
                summaryPipeline.Start,
                peoplePipeline.Output,
                peoplePipeline.EstimatedOutputCount,
                peoplePipeline.Completion,
                true);
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

        private StartableBlock<DataBatch<Person>> CreatePeoplePipeline(string targetFilePath, PeopleSummary aggregatedSummary, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            // Create blocks
            var readBlock = _readPeopleBlockFactory.Create(cancellationSource.Token);
            var writeBlock = _writeCsvBlockFactory.Create<Person>(targetFilePath, true, cancellationSource.Token);
            var buildSummaryBlock = new ProcessingBlock<DataBatch<Person>>(DataflowFacade.TransformBlock("BuildSummary",
                                                                                                         DataBatch<Person>.IdGetter,
                                                                                                         x => x.Data.ForEach(person => _peopleSummaryBuilder.Include(person, aggregatedSummary)),
                                                                                                         cancellationSource.Token));
            var progressBlock = _progressReportingBlockFactory.Create("PersonProgress",
                                                                      DataBatch<Person>.IdGetter,
                                                                      progress,
                                                                      readBlock.EstimatedOutputCount,
                                                                      1,
                                                                      cancellationSource.Token);

            // Create pipeline
            var pipeline = _straightPipelineFactory.Create(readBlock,
                                                           new[] { writeBlock, buildSummaryBlock, progressBlock },
                                                           cancellationSource);

            pipeline.ContinueCompletionWith(_ => cancellationSource.Dispose());

            return pipeline;
        }
    }
}
