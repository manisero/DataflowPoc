using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Extensions
{
    public static class DataflowFacade
    {
        public static BufferBlock<TData> BufferBlock<TData>(CancellationToken cancellationToken, int boundedCapacity = -1)
        {
            return new BufferBlock<TData>(new DataflowBlockOptions
                {
                    CancellationToken = cancellationToken,
                    BoundedCapacity = boundedCapacity
                });
        }

        public static TransformBlock<TData, TData> TransformBlock<TData>(Func<TData, TData> transform,
                                                                         CancellationToken cancellationToken)
            => TransformBlock<TData, TData>(transform, cancellationToken);

        public static TransformBlock<TInput, TOutput> TransformBlock<TInput, TOutput>(Func<TInput, TOutput> transform,
                                                                                      CancellationToken cancellationToken)
        {
            return new TransformBlock<TInput, TOutput>(transform,
                                                       new ExecutionDataflowBlockOptions
                                                           {
                                                               CancellationToken = cancellationToken,
                                                               BoundedCapacity = 1
                                                           });
        }

        public static TransformManyBlock<TInput, TOutput> TransformManyBlock<TInput, TOutput>(Func<TInput, IEnumerable<TOutput>> transform,
                                                                                              CancellationToken cancellationToken)
        {
            return new TransformManyBlock<TInput, TOutput>(transform,
                                                           new ExecutionDataflowBlockOptions
                                                               {
                                                                   CancellationToken = cancellationToken,
                                                                   BoundedCapacity = 1
                                                               });
        }
    }
}
