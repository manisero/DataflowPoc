using System;
using System.IO;
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
        private readonly ISingleItemSourceBlockFactory _singleItemSourceBlockFactory;
        private readonly IPeopleSummaryBuilder _peopleSummaryBuilder;
        private readonly IWriteCsvBlockFactory _writeCsvBlockFactory;
        private readonly IProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly IStraightPipelineFactory _straightPipelineFactory;

        public PipelineFactory(IReadSummaryBlockFactory readSummaryBlockFactory,
                               IReadPeopleBlockFactory readPeopleBlockFactory,
                               ISingleItemSourceBlockFactory singleItemSourceBlockFactory,
                               IPeopleSummaryBuilder peopleSummaryBuilder,
                               IWriteCsvBlockFactory writeCsvBlockFactory,
                               IProgressReportingBlockFactory progressReportingBlockFactory,
                               IStraightPipelineFactory straightPipelineFactory)
        {
            _readSummaryBlockFactory = readSummaryBlockFactory;
            _readPeopleBlockFactory = readPeopleBlockFactory;
            _singleItemSourceBlockFactory = singleItemSourceBlockFactory;
            _peopleSummaryBuilder = peopleSummaryBuilder;
            _writeCsvBlockFactory = writeCsvBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _straightPipelineFactory = straightPipelineFactory;
        }

        public StartableBlock<DataBatch<Person>> Create(string targetFilePath, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            File.Create(targetFilePath).Dispose();
            var builtSummary = new PeopleSummary();

            // Create pipelines
            var summaryFromDbPipeline = CreateSummaryFormDbPipeline(targetFilePath, progress, cancellation);

            var writeEmptyLineBlock1 = new StartableBlock<object>(
                () =>
                    {
                        if (!summaryFromDbPipeline.Completion.IsFaulted && !summaryFromDbPipeline.Completion.IsCanceled)
                        {
                            File.AppendAllLines(targetFilePath, new[] { string.Empty });
                        }
                    });

            var peoplePipeline = CreatePeoplePipeline(targetFilePath, builtSummary, progress, cancellation);

            var writeEmptyLineBlock2 = new StartableBlock<object>(
                () =>
                    {
                        if (!peoplePipeline.Completion.IsFaulted && !peoplePipeline.Completion.IsCanceled)
                        {
                            File.AppendAllLines(targetFilePath, new[] { string.Empty });
                        }
                    });

            var builtSummaryPipeline = CreateBuiltSummaryPipeline(targetFilePath, builtSummary, progress, cancellation);

            // Link pipelines
            summaryFromDbPipeline.ContinueWith(writeEmptyLineBlock1);
            writeEmptyLineBlock1.ContinueWith(peoplePipeline);
            peoplePipeline.ContinueWith(writeEmptyLineBlock2, false);
            writeEmptyLineBlock2.ContinueWith(builtSummaryPipeline);
            builtSummaryPipeline.Output.IgnoreOutput();

            return new StartableBlock<DataBatch<Person>>(
                summaryFromDbPipeline.Start,
                peoplePipeline.Output,
                peoplePipeline.EstimatedOutputCount,
                builtSummaryPipeline.Completion,
                true);
        }

        private StartableBlock<DataBatch<PeopleSummary>> CreateSummaryFormDbPipeline(string targetFilePath, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            // Create blocks
            var readBlock = _readSummaryBlockFactory.Create(cancellationSource.Token);
            var writeBlock = _writeCsvBlockFactory.Create<PeopleSummary>(targetFilePath, true, cancellationSource.Token);
            var progressBlock = _progressReportingBlockFactory.Create("PeopleSummaryFromDbProgress",
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

        private StartableBlock<DataBatch<Person>> CreatePeoplePipeline(string targetFilePath, PeopleSummary builtSummary, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            // Create blocks
            var readBlock = _readPeopleBlockFactory.Create(cancellationSource.Token);
            var writeBlock = _writeCsvBlockFactory.Create<Person>(targetFilePath, true, cancellationSource.Token);
            var buildSummaryBlock = new ProcessingBlock<DataBatch<Person>>(DataflowFacade.TransformBlock("BuildSummary",
                                                                                                         DataBatch<Person>.IdGetter,
                                                                                                         x => x.Data.ForEach(person => _peopleSummaryBuilder.Include(person, builtSummary)),
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

        private StartableBlock<DataBatch<PeopleSummary>> CreateBuiltSummaryPipeline(string targetFilePath, PeopleSummary aggregatedSummary, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            // Create blocks
            var sourceBlock = _singleItemSourceBlockFactory.Create(() => new DataBatch<PeopleSummary>
                                                                       {
                                                                           Number = -1,
                                                                           DataOffset = 0,
                                                                           IntendedSize = 1,
                                                                           Data = new[] { aggregatedSummary }
                                                                       },
                                                                   cancellationSource.Token);
            var writeBlock = _writeCsvBlockFactory.Create<PeopleSummary>(targetFilePath, true, cancellationSource.Token);
            var progressBlock = _progressReportingBlockFactory.Create("BuiltPeopleSummaryProgress",
                                                                      DataBatch<PeopleSummary>.IdGetter,
                                                                      progress,
                                                                      sourceBlock.EstimatedOutputCount,
                                                                      1,
                                                                      cancellationSource.Token);

            // Create pipeline
            var pipeline = _straightPipelineFactory.Create(sourceBlock,
                                                           new[] { writeBlock, progressBlock },
                                                           cancellationSource);

            pipeline.ContinueCompletionWith(_ => cancellationSource.Dispose());

            return pipeline;
        }
    }
}
