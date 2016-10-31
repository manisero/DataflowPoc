﻿using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.PipelineConstruction
{
    public class EmptyBlockFactory
    {
        public ProcessingBlock<TData> Create<TData>(CancellationToken cancellation)
        {
            // Create blocks
            var emptyBlock = new BufferBlock<TData>(new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            return new ProcessingBlock<TData>
                {
                    Processor = emptyBlock,
                    Completion = emptyBlock.Completion
                };
        }
    }
}
