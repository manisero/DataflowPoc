using System;
using System.Threading;
using Dataflow.Models;
using Dataflow.Pipelines.BlockFactories;

namespace Dataflow.Pipelines
{
    public class PeoplePipelineFactory
    {
        private readonly ReadingBlockFactory _readingBlockFactory;
        private readonly WritingBlockFactory _writingBlockFactory;
        private readonly ThrowingBlockFactory _throwingBlockFactory;
        private readonly EmptyBlockFactory _emptyBlockFactory;
        private readonly ProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly PipelineFactory _pipelineFactory;

        public PeoplePipelineFactory(ReadingBlockFactory readingBlockFactory,
                                     WritingBlockFactory writingBlockFactory,
                                     ThrowingBlockFactory throwingBlockFactory,
                                     EmptyBlockFactory emptyBlockFactory,
                                     ProgressReportingBlockFactory progressReportingBlockFactory,
                                     PipelineFactory pipelineFactory)
        {
            _readingBlockFactory = readingBlockFactory;
            _writingBlockFactory = writingBlockFactory;
            _throwingBlockFactory = throwingBlockFactory;
            _emptyBlockFactory = emptyBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _pipelineFactory = pipelineFactory;
        }

        public StartableBlock<Data> Create(string peopleJsonFilePath, string targetFilePath, IProgress<PipelineProgress> progress, CancellationTokenSource cancellationSource)
        {
            // Create blocks

            // TODO: Unified Dataflow blocks creation (BoundedCapacity etc.)
            // TODO: Performance measurement

            // TODO: Progress reporting approach 1: before anything
            var readBlock = _readingBlockFactory.Create(peopleJsonFilePath, cancellationSource.Token);
            var writeBlock = _writingBlockFactory.Create(targetFilePath, readBlock.EstimatedOutputCount, cancellationSource.Token);
            var throwBlock = Settings.ThrowTest
                                 ? _throwingBlockFactory.Create<Data>(cancellationSource.Token)
                                 : _emptyBlockFactory.Create<Data>(writeBlock.EstimatedOutputCount, cancellationSource.Token);
            // TODO: Data-level error handling (reporting / logging)
            var progressBlock = _progressReportingBlockFactory.Create<Data>(progress, throwBlock.EstimatedOutputCount, cancellationSource.Token);
            
            return _pipelineFactory.Create(readBlock,
                                           new[] { writeBlock, throwBlock, progressBlock },
                                           cancellationSource);
        }
    }
}
