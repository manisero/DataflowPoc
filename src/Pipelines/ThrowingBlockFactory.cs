using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Pipelines
{
    public class ThrowingBlockFactory
    {
        public ProcessingBlock<TData> Create<TData>(CancellationToken cancellation)
        {
            Func<TData, TData> transform = x =>
                                               {
                                                   throw new InvalidOperationException();
                                               };

            // Create blocks
            var throwBlock = new TransformBlock<TData, TData>(transform,
                                                              new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            return new ProcessingBlock<TData>
                {
                    Processor = throwBlock,
                    Completion = throwBlock.Completion
                };
        }
    }
}
