using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Manisero.DataflowPoc.Core.Extensions;

namespace Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks
{
    public class StartableBlock<TOutput>
    {
        public Action Start { get; set; }

        public ISourceBlock<TOutput> Output { get; set; }

        public int EstimatedOutputCount { get; set; }

        public Task Completion { get; set; }
    }

    public static class StartableBlockExtensions
    {
        public static void ContinueWith<TOutput, TContinuationOutput>(this StartableBlock<TOutput> block, StartableBlock<TContinuationOutput> continuationBlock)
        {
            block.Output.IgnoreOutput();

            block.Completion
                 .ContinueWith(x =>
                                   {
                                       if (x.IsFaulted)
                                       {
                                           continuationBlock.Output.Fault(x.Exception);
                                       }
                                       else
                                       {
                                           continuationBlock.Start();
                                       }
                                   });
        }
    }
}
