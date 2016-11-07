using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.DataflowPoc.Extensions;
using Manisero.DataflowPoc.Logic;
using Manisero.DataflowPoc.Models;
using Manisero.DataflowPoc.Pipelines.PipelineBlocks;

namespace Manisero.DataflowPoc.Pipelines.PeopleBatchesStream.BlockFactories
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

        public StartableBlock<DataBatch> Create(string peopleJsonFilePath, DataPool dataPool, CancellationToken cancellation)
        {
            var batchSize = Settings.ReadingBatchSize;
            var peopleCount = _fileLinesCounter.Count(peopleJsonFilePath);
            var batchesCount = peopleCount.CeilingOfDivisionBy(batchSize);

            var peopleJsonStream = File.OpenText(peopleJsonFilePath);

            // Create blocks
            var bufferBlock = DataflowFacade.BufferBlock<DataBatch>(cancellation);
            var readBlock = Settings.SplitReadingIntoTwoSteps
                                ? UseLinesReaderAndParser(peopleJsonStream, dataPool, cancellation)
                                : UseDataReader(peopleJsonStream, dataPool, cancellation);

            //  Link blocks
            bufferBlock.LinkWithCompletion(readBlock);

            // Handle completion
            var completion = readBlock.Completion.ContinueWithStatusPropagation(_ => peopleJsonStream.Dispose());

            return new StartableBlock<DataBatch>
                {
                    Start = () =>
                                {
                                    for (var i = 0; i < batchesCount; i++)
                                    {
                                        bufferBlock.Post(new DataBatch { IntendedSize = batchSize });
                                    }

                                    bufferBlock.Complete();
                                },
                    Output = readBlock,
                    EstimatedOutputCount = batchesCount,
                    Completion = completion
                };
        }

        private IPropagatorBlock<DataBatch, DataBatch> UseDataReader(StreamReader peopleJsonStream, DataPool dataPool, CancellationToken cancellation)
        {
            return DataflowFacade.TransformBlock<DataBatch, DataBatch>(
                "ReadData",
                DataBatch.IdGetter,
                x =>
                    {
                        x.Data = _dataReader.Read(peopleJsonStream, x.IntendedSize, dataPool).ToList();
                        return x;
                    },
                cancellation);
        }

        private IPropagatorBlock<DataBatch, DataBatch> UseLinesReaderAndParser(StreamReader peopleJsonStream, DataPool dataPool, CancellationToken cancellation)
        {
            // Create blocks

            // NOTE:
            // - extract part which must be single-thread
            // - ability to replace just reading (e.g. from db)
            var readLinesBlock = DataflowFacade.TransformBlock<DataBatch, DataBatch>(
                "ReadLines",
                DataBatch.IdGetter,
                x =>
                    {
                        x.Data = _streamLinesReader.Read(peopleJsonStream, x.IntendedSize)
                                                   .Select(line =>
                                                               {
                                                                   var data = dataPool.Rent();
                                                                   data.PersonJson = line;

                                                                   return data;
                                                               })
                                                   .ToList();
                        return x;
                    },
                cancellation);

            // NOTE: can be multi-thread
            var parseDataBlock = DataflowFacade.TransformBlock(
                "ParseData",
                DataBatch.IdGetter,
                x => x.Data.ForEach(_dataParser.Parse),
                cancellation);

            // Link blocks
            readLinesBlock.LinkWithCompletion(parseDataBlock);

            return DataflowBlock.Encapsulate(readLinesBlock, parseDataBlock);
        }
    }
}
