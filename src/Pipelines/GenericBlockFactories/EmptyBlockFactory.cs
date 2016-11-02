using System.Threading;
using Dataflow.Extensions;
using Dataflow.Pipelines.PipelineBlocks;

namespace Dataflow.Pipelines.GenericBlockFactories
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
