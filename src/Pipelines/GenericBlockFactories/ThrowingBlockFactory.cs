using System;
using System.Threading;
using Dataflow.Extensions;
using Dataflow.Pipelines.PipelineBlocks;

namespace Dataflow.Pipelines.GenericBlockFactories
{
    public class ThrowingBlockFactory
    {
        public ProcessingBlock<TData> Create<TData>(Func<TData, object> dataIdGetter, CancellationToken cancellation)
        {
            // Create blocks
            var throwBlock = DataflowFacade.TransformBlock(
                "Throw",
                x =>
                    {
                        throw new InvalidOperationException();
                    },
                dataIdGetter,
                cancellation);

            return new ProcessingBlock<TData>
                {
                    Processor = throwBlock,
                    Completion = throwBlock.Completion
                };
        }
    }
}
