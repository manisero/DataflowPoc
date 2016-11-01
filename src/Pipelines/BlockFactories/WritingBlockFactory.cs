﻿using System.IO;
using System.Threading;
using Dataflow.Extensions;
using Dataflow.Logic;
using Dataflow.Models;

namespace Dataflow.Pipelines.BlockFactories
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
            var writingBlock = DataflowFacade.TransformBlock<Data>(
                x =>
                    {
                        _dataWriter.Write(writer, x);
                        return x;
                    },
                cancellation);

            // Handle completion
            var completion = writingBlock.Completion.ContinueWithStatusPropagation(x => writer.Dispose());

            return new ProcessingBlock<Data>
                {
                    Processor = writingBlock,
                    Completion = completion
                };
        }
    }
}
