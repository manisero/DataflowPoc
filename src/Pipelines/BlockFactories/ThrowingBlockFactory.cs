using System;
using System.Threading;
using Dataflow.Extensions;

namespace Dataflow.Pipelines.BlockFactories
{
    public class ThrowingBlockFactory
    {
        public ProcessingBlock<TData> Create<TData>(CancellationToken cancellation)
        {
            // Create blocks
            var throwBlock = DataflowFacade.TransformBlock<TData>(
                "Throw",
                x =>
                    {
                        throw new InvalidOperationException();
                    },
                cancellation);

            return new ProcessingBlock<TData>
                {
                    Processor = throwBlock,
                    Completion = throwBlock.Completion
                };
        }
    }
}
