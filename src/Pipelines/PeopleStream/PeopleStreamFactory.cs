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

        public StartableBlock<Data> Create(string peopleJsonFilePath, string targetFilePath, CancellationToken cancellation)
        {
            // Create blocks
            var readBlock = _readingBlockFactory.Create(peopleJsonFilePath, cancellation);
            var writeBlock = _writingBlockFactory.Create(targetFilePath, readBlock.EstimatedOutputCount, cancellation);

            // Link blocks
            readBlock.Output.LinkWithCompletion(writeBlock.Processor);

            return new StartableBlock<Data>
                {
                    Start = readBlock.Start,
                    Output = writeBlock.Processor,
                    EstimatedOutputCount = writeBlock.EstimatedOutputCount,
                    Completion = null // TODO: Merge read & write completion
                };
        }
    }
}
