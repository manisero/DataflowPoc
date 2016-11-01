using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Pipelines.BlockFactories
{
    public class EmptyBlockFactory
    {
        public ProcessingBlock<TData> Create<TData>(int estimatedInputCount, CancellationToken cancellation)
        {
            // Create blocks
            var emptyBlock = new BufferBlock<TData>(new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            return new ProcessingBlock<TData>
                {
                    Processor = emptyBlock,
                    EstimatedOutputCount = estimatedInputCount,
                    Completion = emptyBlock.Completion
                };
        }
    }
}
