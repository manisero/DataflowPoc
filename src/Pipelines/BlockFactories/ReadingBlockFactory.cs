using System.IO;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Dataflow.Extensions;
using Dataflow.Logic;
using Dataflow.Models;

namespace Dataflow.Pipelines.BlockFactories
{
    public class ReadingBlockFactory
    {
        private readonly FileLinesCounter _fileLinesCounter;
        private readonly DataReader _dataReader;
        private readonly StreamLinesReader _streamLinesReader;
        private readonly DataParser _dataParser;

        public ReadingBlockFactory(FileLinesCounter fileLinesCounter,
                                   DataReader dataReader,
                                   StreamLinesReader streamLinesReader,
                                   DataParser dataParser)
        {
            _fileLinesCounter = fileLinesCounter;
            _dataReader = dataReader;
            _streamLinesReader = streamLinesReader;
            _dataParser = dataParser;
        }

        public StartableBlock<Data> Create(string peopleJsonFilePath, CancellationToken cancellation)
        {
            var batchSize = Settings.ReadingBatchSize;
            var peopleCount = _fileLinesCounter.Count(peopleJsonFilePath);
            var batchesCount = peopleCount.CeilingOfDivisionBy(batchSize);

            var peopleJsonStream = File.OpenText(peopleJsonFilePath);

            // Create blocks
            var bufferBlock = DataflowFacade.BufferBlock<int>(cancellation);
            var readBlock = Settings.OptimizeReading
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
                                        bufferBlock.Post(batchSize);
                                    }

                                    bufferBlock.Complete();
                                },
                    Output = readBlock,
                    EstimatedOutputCount = peopleCount,
                    Completion = completion
                };
        }

        private IPropagatorBlock<int, Data> UseDataReader(StreamReader peopleJsonStream, CancellationToken cancellation)
        {
            return DataflowFacade.TransformManyBlock<int, Data>(
                "ReadData",
                x => _dataReader.Read(peopleJsonStream, x),
                cancellation);
        }

        private IPropagatorBlock<int, Data> UseLinesReaderAndParser(StreamReader peopleJsonStream, CancellationToken cancellation)
        {
            // Create blocks

            // NOTE:
            // - extract part which must be single-thread
            // - ability to replace just reading (e.g. from db)
            var readLinesBlock = DataflowFacade.TransformManyBlock<int, string>(
                "ReadLines",
                x => _streamLinesReader.Read(peopleJsonStream, x),
                cancellation);

            // NOTE: can be multi-thread
            var parseDataBlock = DataflowFacade.TransformBlock<string, Data>(
                "ParseData",
                x => _dataParser.Parse(x),
                cancellation);

            // Link blocks
            readLinesBlock.LinkWithCompletion(parseDataBlock);

            return DataflowBlock.Encapsulate(readLinesBlock, parseDataBlock);
        }
    }
}
