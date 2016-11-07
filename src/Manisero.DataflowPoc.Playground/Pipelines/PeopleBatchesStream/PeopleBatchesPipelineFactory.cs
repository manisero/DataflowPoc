using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines;
using Manisero.DataflowPoc.Core.Pipelines.GenericBlockFactories;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.Playground.Logic;
using Manisero.DataflowPoc.Playground.Models;
using Manisero.DataflowPoc.Playground.Pipelines.PeopleBatchesStream.BlockFactories;

namespace Manisero.DataflowPoc.Playground.Pipelines.PeopleBatchesStream
{
    public class PeopleBatchesPipelineFactory
    {
        private readonly ReadingBlockFactory _readingBlockFactory;
        private readonly PersonValidator _personValidator;
        private readonly PersonFieldsComputer _personFieldsComputer;
        private readonly WritingBlockFactory _writingBlockFactory;
        private readonly ProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly StraightPipelineFactory _straightPipelineFactory;

        public PeopleBatchesPipelineFactory(ReadingBlockFactory readingBlockFactory,
                                     PersonValidator personValidator,
                                     PersonFieldsComputer personFieldsComputer,
                                     WritingBlockFactory writingBlockFactory,
                                     ProgressReportingBlockFactory progressReportingBlockFactory,
                                     StraightPipelineFactory straightPipelineFactory)
        {
            _readingBlockFactory = readingBlockFactory;
            _personValidator = personValidator;
            _personFieldsComputer = personFieldsComputer;
            _writingBlockFactory = writingBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _straightPipelineFactory = straightPipelineFactory;
        }

        public StartableBlock<DataBatch> Create(string peopleJsonFilePath,
                                                string targetFilePath,
                                                IProgress<PipelineProgress> progress,
                                                CancellationTokenSource cancellationSource)
        {
            var dataPool = new DataPool();

            // Create blocks
            var readBlock = _readingBlockFactory.Create(peopleJsonFilePath, dataPool, cancellationSource.Token);
            var validateBlock = ProcessingBlock<DataBatch>.Create("Validate",
                                                                  DataBatch.IdGetter,
                                                                  x => x.Data.Where(item => item.IsValid).ForEach(_personValidator.Validate),
                                                                  cancellationSource.Token,
                                                                  Settings.ProcessInParallel ? Settings.MaxDegreeOfParallelism : 1);
            var computeFieldsBlock = ProcessingBlock<DataBatch>.Create("ComputeFields",
                                                                       DataBatch.IdGetter,
                                                                       x => x.Data.Where(item => item.IsValid).ForEach(_personFieldsComputer.Compute),
                                                                       cancellationSource.Token,
                                                                       Settings.ProcessInParallel ? Settings.MaxDegreeOfParallelism : 1);
            var extraProcessingBlocks = CreateExtraProcessingBlocks(cancellationSource);
            var writeBlock = _writingBlockFactory.Create(targetFilePath, cancellationSource.Token);
            var progressBlock = _progressReportingBlockFactory.Create(DataBatch.IdGetter, progress, readBlock.EstimatedOutputCount, 1, cancellationSource.Token);
            var disposeBlock = ProcessingBlock<DataBatch>.Create("DisposeData",
                                                                 DataBatch.IdGetter,
                                                                 x => x.Data.ForEach(dataPool.Return),
                                                                 cancellationSource.Token);

            return _straightPipelineFactory.Create(readBlock,
                                                   new[] { validateBlock, computeFieldsBlock }.Concat(extraProcessingBlocks)
                                                                                              .Concat(new[] { writeBlock, progressBlock, disposeBlock })
                                                                                              .ToArray(),
                                                   cancellationSource);
        }

        private IEnumerable<ProcessingBlock<DataBatch>> CreateExtraProcessingBlocks(CancellationTokenSource cancellationSource)
        {
            return Enumerable.Range(1, Settings.ExtraProcessingBlocksCount)
                             .Select(x => ProcessingBlock<DataBatch>.Create($"ExtraProcessing {x}",
                                                                            DataBatch.IdGetter,
                                                                            batch => batch.Data.ForEach(_ => ComputationsHelper.PerformTimeConsumingOperation()),
                                                                            cancellationSource.Token,
                                                                            Settings.ProcessInParallel ? Settings.MaxDegreeOfParallelism : 1));
        }
    }
}
