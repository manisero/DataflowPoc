﻿using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Logic;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories
{
    public interface IReadPeopleBlockFactory
    {
        StartableBlock<DataBatch<Person>> Create(CancellationToken cancellation);
    }

    public class ReadPeopleBlockFactory : IReadPeopleBlockFactory
    {
        private readonly IPeopleCounter _peopleCounter;
        private readonly IPeopleBatchReader _peopleBatchReader;

        public ReadPeopleBlockFactory(IPeopleCounter peopleCounter,
                                      IPeopleBatchReader peopleBatchReader)
        {
            _peopleCounter = peopleCounter;
            _peopleBatchReader = peopleBatchReader;
        }

        public StartableBlock<DataBatch<Person>> Create(CancellationToken cancellation)
        {
            var batchSize = Settings.ReadingBatchSize;
            var batchesCount = new Lazy<int>(() => GetBatchesCount(batchSize));

            // Create blocks
            var bufferBlock = DataflowFacade.BufferBlock<DataBatch<Person>>(cancellation);

            var readBlock = DataflowFacade.TransformBlock<DataBatch<Person>>("ReadPerson",
                                                                             DataBatch<Person>.IdGetter,
                                                                             x => x.Data = _peopleBatchReader.Read(x.DataOffset, x.IntendedSize),
                                                                             cancellation);

            // Link blocks
            bufferBlock.LinkWithCompletion(readBlock);

            return new StartableBlock<DataBatch<Person>>(
                () => Start(batchSize, batchesCount.Value, bufferBlock),
                readBlock,
                batchesCount);
        }

        private int GetBatchesCount(int batchSize)
        {
            var peopleCount = _peopleCounter.Count();

            return peopleCount.CeilingOfDivisionBy(batchSize);
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
