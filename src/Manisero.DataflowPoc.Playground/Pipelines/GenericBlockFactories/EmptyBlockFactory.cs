using System.Threading;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Pipelines.PipelineBlocks;

namespace Manisero.DataflowPoc.Pipelines.GenericBlockFactories
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
