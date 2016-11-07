using System;
using System.Threading;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;

namespace Manisero.DataflowPoc.Core.Pipelines.GenericBlockFactories
{
    public class ThrowingBlockFactory
    {
        public ProcessingBlock<TData> Create<TData>(Func<TData, int> dataIdGetter, CancellationToken cancellation)
        {
            // Create blocks
            var throwBlock = DataflowFacade.TransformBlock(
                "Throw",
                dataIdGetter,
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
