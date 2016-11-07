using System.IO;
using System.Threading;
using Manisero.DataflowPoc.Extensions;
using Manisero.DataflowPoc.Logic;
using Manisero.DataflowPoc.Models;
using Manisero.DataflowPoc.Pipelines.PipelineBlocks;

namespace Manisero.DataflowPoc.Pipelines.PeopleBatchesStream.BlockFactories
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

            return new ProcessingBlock<DataBatch>
                {
                    Processor = writeBlock,
                    Completion = completion
                };
        }
    }
}
