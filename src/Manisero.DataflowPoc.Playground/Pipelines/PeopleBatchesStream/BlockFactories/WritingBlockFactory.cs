using System.IO;
using System.Threading;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.Playground.Logic;
using Manisero.DataflowPoc.Playground.Models;

namespace Manisero.DataflowPoc.Playground.Pipelines.PeopleBatchesStream.BlockFactories
{
    public class WritingBlockFactory
    {
        private readonly DataWriter _dataWriter;

        public WritingBlockFactory(DataWriter dataWriter)
        {
            _dataWriter = dataWriter;
        }

        public ProcessingBlock<DataBatch> Create(string targetFilePath, CancellationToken cancellation)
        {
            var targetDirectoryPath = Path.GetDirectoryName(targetFilePath);

            if (!Directory.Exists(targetDirectoryPath))
            {
                Directory.CreateDirectory(targetDirectoryPath);
            }

            var writer = new StreamWriter(targetFilePath);

            // Create blocks
            var writeBlock = DataflowFacade.TransformBlock(
                "WriteData",
                DataBatch.IdGetter,
                x => x.Data.ForEach(item => _dataWriter.Write(writer, item)),
                cancellation);

            // Handle completion
            var completion = writeBlock.Completion.ContinueWithStatusPropagation(_ => writer.Dispose());

            return new ProcessingBlock<DataBatch>(writeBlock, completion);
        }
    }
}
