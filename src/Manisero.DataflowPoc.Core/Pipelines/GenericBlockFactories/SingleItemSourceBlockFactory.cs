using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;

namespace Manisero.DataflowPoc.Core.Pipelines.GenericBlockFactories
{
    public interface ISingleItemSourceBlockFactory
    {
        StartableBlock<TOutput> Create<TOutput>(Func<TOutput> itemGetter, CancellationToken cancellation);
    }

    public class SingleItemSourceBlockFactory : ISingleItemSourceBlockFactory
    {
        public StartableBlock<TOutput> Create<TOutput>(Func<TOutput> itemGetter, CancellationToken cancellation)
        {
            var sourceBuffer = DataflowFacade.BufferBlock<TOutput>(cancellation);

            return new StartableBlock<TOutput>(() =>
                                                   {
                                                       sourceBuffer.Post(itemGetter());
                                                       sourceBuffer.Complete();
                                                   },
                                               sourceBuffer,
                                               1);
        }
    }
}
