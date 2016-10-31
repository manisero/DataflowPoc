﻿using System.IO;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Dataflow.Extensions;
using Dataflow.Logic;
using Dataflow.Models;

namespace Dataflow.PipelineConstruction
{
    public class ReadingBlockFactory
    {
        private const int BATCH_SIZE = 1000;

        private readonly bool _optimize;
        private readonly FileLinesCounter _fileLinesCounter;
        private readonly DataReader _dataReader;
        private readonly StreamLinesReader _streamLinesReader;
        private readonly DataParser _dataParser;

        public ReadingBlockFactory(bool optimize, FileLinesCounter fileLinesCounter, DataReader dataReader, StreamLinesReader streamLinesReader, DataParser dataParser)
        {
            _optimize = optimize;
            _fileLinesCounter = fileLinesCounter;
            _dataReader = dataReader;
            _streamLinesReader = streamLinesReader;
            _dataParser = dataParser;
        }

        public StartableBlock<Data> Create(string peopleJsonFilePath, CancellationToken cancellation)
        {
            var peopleCount = _fileLinesCounter.Count(peopleJsonFilePath);
            var batchesCount = peopleCount.CeilingOfDivisionBy(BATCH_SIZE);

            var peopleJsonStream = File.OpenText(peopleJsonFilePath);

            // Create blocks
            var bufferBlock = new BufferBlock<int>(new DataflowBlockOptions { CancellationToken = cancellation });
            var readBlock = _optimize
                                ? UseLinesReaderAndParser(peopleJsonStream, cancellation)
                                : UseDataReader(peopleJsonStream, cancellation);

            //  Link blocks
            bufferBlock.LinkWithCompletion(readBlock);

            // Handle completion
            var completion = readBlock.Completion.ContinueWithStatusPropagation(_ => peopleJsonStream.Dispose());

            return new StartableBlock<Data>
                {
                    Start = () =>
                                {
                                    for (var i = 0; i < batchesCount; i++)
                                    {
                                        bufferBlock.Post(BATCH_SIZE);
                                    }

                                    bufferBlock.Complete();
                                },
                    Output = readBlock,
                    EstimatedOutputCount = peopleCount,
                    Completion = completion
                };
        }

        public IPropagatorBlock<int, Data> UseDataReader(StreamReader peopleJsonStream, CancellationToken cancellation)
        {
            return new TransformManyBlock<int, Data>(x => _dataReader.Read(peopleJsonStream, x), new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });
        }

        public IPropagatorBlock<int, Data> UseLinesReaderAndParser(StreamReader peopleJsonStream, CancellationToken cancellation)
        {
            // Create blocks
            
            // NOTE:
            // - extract part which must be single-thread
            // - ability to replace just reading (e.g. from db)
            var readLinesBlock = new TransformManyBlock<int, string>(x => _streamLinesReader.Read(peopleJsonStream, x), new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            // NOTE: can be multi-thread
            var parseDataBlock = new TransformBlock<string, Data>(x => _dataParser.Parse(x), new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            // Link blocks
            readLinesBlock.LinkWithCompletion(parseDataBlock);

            return DataflowBlock.Encapsulate(readLinesBlock, parseDataBlock);
        }
    }
}
