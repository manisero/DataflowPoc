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
        private readonly PipelineFactory _pipelineFactory;

        public PeoplePipelineFactory(ReadingBlockFactory readingBlockFactory,
                                     WritingBlockFactory writingBlockFactory,
                                     ThrowingBlockFactory throwingBlockFactory,
                                     EmptyBlockFactory emptyBlockFactory,
                                     PipelineFactory pipelineFactory)
        {
            _readingBlockFactory = readingBlockFactory;
            _writingBlockFactory = writingBlockFactory;
            _throwingBlockFactory = throwingBlockFactory;
            _emptyBlockFactory = emptyBlockFactory;
            _pipelineFactory = pipelineFactory;
        }

        public StartableBlock<Data> Create(string peopleJsonFilePath, string targetFilePath, CancellationTokenSource cancellationSource)
        {
            // Create blocks
            // TODO: Progress reporting approach 1: before anything
            var readBlock = _readingBlockFactory.Create(peopleJsonFilePath, cancellationSource.Token);
            var writeBlock = _writingBlockFactory.Create(targetFilePath, readBlock.EstimatedOutputCount, cancellationSource.Token);
            var throwBlock = Settings.ThrowTest
                                 ? _throwingBlockFactory.Create<Data>(cancellationSource.Token)
                                 : _emptyBlockFactory.Create<Data>(writeBlock.EstimatedOutputCount, cancellationSource.Token);
            // TODO: Data-level error handling (reporting / logging)
            // TODO: Progress reporting approach 2: after everything

            return _pipelineFactory.Create(readBlock, new[] { writeBlock, throwBlock }, cancellationSource);
        }
    }
}
