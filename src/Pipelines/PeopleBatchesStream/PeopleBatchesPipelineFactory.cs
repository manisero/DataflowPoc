using System;
using System.Linq;
using System.Threading;
using Dataflow.Extensions;
using Dataflow.Logic;
using Dataflow.Models;
using Dataflow.Pipelines.GenericBlockFactories;
using Dataflow.Pipelines.PeopleBatchesStream.BlockFactories;
using Dataflow.Pipelines.PipelineBlocks;

namespace Dataflow.Pipelines.PeopleBatchesStream
{
    public class PeopleBatchesPipelineFactory
    {
        private readonly ReadingBlockFactory _readingBlockFactory;
        private readonly PersonValidator _personValidator;
        private readonly PersonFieldsComputer _personFieldsComputer;
        private readonly WritingBlockFactory _writingBlockFactory;
        private readonly ThrowingBlockFactory _throwingBlockFactory;
        private readonly EmptyBlockFactory _emptyBlockFactory;
        private readonly ProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly StraightPipelineFactory _straightPipelineFactory;

        public PeopleBatchesPipelineFactory(ReadingBlockFactory readingBlockFactory,
                                     PersonValidator personValidator,
                                     PersonFieldsComputer personFieldsComputer,
                                     WritingBlockFactory writingBlockFactory,
                                     ThrowingBlockFactory throwingBlockFactory,
                                     EmptyBlockFactory emptyBlockFactory,
                                     ProgressReportingBlockFactory progressReportingBlockFactory,
                                     StraightPipelineFactory straightPipelineFactory)
        {
            _readingBlockFactory = readingBlockFactory;
            _personValidator = personValidator;
            _personFieldsComputer = personFieldsComputer;
            _writingBlockFactory = writingBlockFactory;
            _throwingBlockFactory = throwingBlockFactory;
            _emptyBlockFactory = emptyBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _straightPipelineFactory = straightPipelineFactory;
        }

        public StartableBlock<DataBatch> Create(string peopleJsonFilePath,
                                                string targetFilePath,
                                                IProgress<PipelineProgress> progress,
                                                CancellationTokenSource cancellationSource)
        {
            // Create blocks
            var readBlock = _readingBlockFactory.Create(peopleJsonFilePath, cancellationSource.Token);
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
            var writeBlock = _writingBlockFactory.Create(targetFilePath, cancellationSource.Token);
            var throwBlock = Settings.ThrowTest
                                 ? _throwingBlockFactory.Create(DataBatch.IdGetter, cancellationSource.Token)
                                 : _emptyBlockFactory.Create<DataBatch>(cancellationSource.Token);
            var progressBlock = _progressReportingBlockFactory.Create(DataBatch.IdGetter, progress, readBlock.EstimatedOutputCount, 1, cancellationSource.Token);

            return _straightPipelineFactory.Create(readBlock,
                                                   new[] { validateBlock, computeFieldsBlock, writeBlock, throwBlock, progressBlock },
                                                   cancellationSource);
        }
    }
}
