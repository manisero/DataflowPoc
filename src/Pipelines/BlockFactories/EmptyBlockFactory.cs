using System.Threading;
using Dataflow.Extensions;

namespace Dataflow.Pipelines.BlockFactories
{
    public class EmptyBlockFactory
    {
        public ProcessingBlock<TData> Create<TData>(CancellationToken cancellation)
        {
            // Create blocks
            var emptyBlock = DataflowFacade.BufferBlock<TData>(cancellation, 1);

            return new ProcessingBlock<TData>
                {
                    Processor = emptyBlock,
                    Completion = emptyBlock.Completion
                };
        }
    }
}
