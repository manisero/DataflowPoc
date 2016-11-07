using System.IO;
using System.Threading;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.Playground.Logic;
using Manisero.DataflowPoc.Playground.Models;

namespace Manisero.DataflowPoc.Playground.Pipelines.PeopleStream.BlockFactories
{
    public class WritingBlockFactory
    {
        private readonly DataWriter _dataWriter;

        public WritingBlockFactory(DataWriter dataWriter)
        {
            _dataWriter = dataWriter;
        }

        public ProcessingBlock<Data> Create(string targetFilePath, CancellationToken cancellation)
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
                Data.IdGetter,
                x => _dataWriter.Write(writer, x),
                cancellation);

            // Handle completion
            var completion = writeBlock.Completion.ContinueWithStatusPropagation(x => writer.Dispose());

            return new ProcessingBlock<Data>
                {
                    Processor = writeBlock,
                    Completion = completion
                };
        }
    }
}
