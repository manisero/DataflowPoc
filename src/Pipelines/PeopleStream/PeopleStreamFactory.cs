using System.Threading;
using Dataflow.Extensions;
using Dataflow.Models;

namespace Dataflow.Pipelines.PeopleStream
{
    public class PeopleStreamFactory
    {
        private readonly ReadingBlockFactory _readingBlockFactory;
        private readonly WritingBlockFactory _writingBlockFactory;

        public PeopleStreamFactory(ReadingBlockFactory readingBlockFactory, WritingBlockFactory writingBlockFactory)
        {
            _readingBlockFactory = readingBlockFactory;
            _writingBlockFactory = writingBlockFactory;
        }

        public StartableBlock<Data> Create(string peopleJsonFilePath, string targetFilePath, CancellationTokenSource cancellationSource)
        {
            // Create blocks
            var readBlock = _readingBlockFactory.Create(peopleJsonFilePath, cancellationSource.Token);
            var writeBlock = _writingBlockFactory.Create(targetFilePath, readBlock.EstimatedOutputCount, cancellationSource.Token);

            // Link blocks
            readBlock.Output.LinkWithCompletion(writeBlock.Processor);

            return new StartableBlock<Data>
                {
                    Start = readBlock.Start,
                    Output = writeBlock.Processor,
                    EstimatedOutputCount = writeBlock.EstimatedOutputCount,
                    Completion = TaskExtensions.CreateGlobalCompletion(cancellationSource,
                                                                       readBlock.Completion, writeBlock.Completion)
                };
        }
    }
}
