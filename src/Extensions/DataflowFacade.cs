using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Dataflow.Etw;

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

        public static TransformBlock<TData, TData> TransformBlock<TData>(string name,
                                                                         Func<TData, TData> transform,
                                                                         CancellationToken cancellationToken)
            => TransformBlock<TData, TData>(name, transform, cancellationToken);

        public static TransformBlock<TInput, TOutput> TransformBlock<TInput, TOutput>(string name,
                                                                                      Func<TInput, TOutput> transform,
                                                                                      CancellationToken cancellationToken)
        {
            return new TransformBlock<TInput, TOutput>(x =>
                                                           {
                                                               Events.Write.BlockEnter(name);
                                                               var sw = Stopwatch.StartNew();
                                                               var o = transform(x);
                                                               Events.Write.BlockExit(name, sw.ElapsedTicks);
                                                               return o;
                                                           },
                                                       new ExecutionDataflowBlockOptions
                                                           {
                                                               CancellationToken = cancellationToken,
                                                               BoundedCapacity = 1
                                                           });
        }

        public static TransformManyBlock<TInput, TOutput> TransformManyBlock<TInput, TOutput>(string name,
                                                                                              Func<TInput, IEnumerable<TOutput>> transform,
                                                                                              CancellationToken cancellationToken)
        {
            return new TransformManyBlock<TInput, TOutput>(x =>
                                                               {
                                                                   Events.Write.BlockEnter(name);
                                                                   var sw = Stopwatch.StartNew();
                                                                   var o = transform(x);
                                                                   Events.Write.BlockExit(name, sw.ElapsedTicks);
                                                                   return o;
                                                               },
                                                           new ExecutionDataflowBlockOptions
                                                               {
                                                                   CancellationToken = cancellationToken,
                                                                   BoundedCapacity = 1
                                                               });
        }
    }
}
