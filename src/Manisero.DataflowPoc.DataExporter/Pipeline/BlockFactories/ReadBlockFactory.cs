using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Logic;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories
{
    public interface IReadBlockFactory
    {
        StartableBlock<DataBatch<Person>> Create(CancellationToken cancellation);
    }

    public class ReadBlockFactory : IReadBlockFactory
    {
        private const int BATCH_SIZE = 100000;

        private readonly IPeopleCounter _peopleCounter;
        private readonly IPeopleBatchReader _peopleBatchReader;

        public ReadBlockFactory(IPeopleCounter peopleCounter,
                                IPeopleBatchReader peopleBatchReader)
        {
            _peopleCounter = peopleCounter;
            _peopleBatchReader = peopleBatchReader;
        }

        public StartableBlock<DataBatch<Person>> Create(CancellationToken cancellation)
        {
            var peopleCount = _peopleCounter.Count();
            var batchesCount = peopleCount.CeilingOfDivisionBy(BATCH_SIZE);

            // Create blocks
            var bufferBlock = DataflowFacade.BufferBlock<DataBatch<Person>>(cancellation);

            var readBlock = DataflowFacade.TransformBlock<DataBatch<Person>>(
                "ReadPeopleBatch",
                x => x.Number,
                x => x.Data = _peopleBatchReader.Read(x.DataOffset, x.IntendedSize),
                cancellation);

            // Link blocks
            bufferBlock.LinkWithCompletion(readBlock);

            return new StartableBlock<DataBatch<Person>>
                {
                    Start = () => Start(BATCH_SIZE, batchesCount, bufferBlock),
                    Output = readBlock,
                    Completion = readBlock.Completion,
                    EstimatedOutputCount = batchesCount
                };
        }

        private void Start(int batchSize, int batchesCount, ITargetBlock<DataBatch<Person>> inputBlock)
        {
            for (var i = 0; i < batchesCount; i++)
            {
                var batch = new DataBatch<Person>
                    {
                        Number = i + 1,
                        DataOffset = i * batchSize,
                        IntendedSize = batchSize
                    };

                inputBlock.Post(batch);
            }

            inputBlock.Complete();
        }
    }
}
