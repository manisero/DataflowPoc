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

        public StartableBlock(Action start, ISourceBlock<TOutput> output, int estimatedOutputCount, Task completion, bool isFirstBlock)
        {
            Start = isFirstBlock ? WrapStart(start, output) : start;
            Output = output;
            EstimatedOutputCount = estimatedOutputCount;
            Completion = completion;
        }

        public StartableBlock(Action start, ISourceBlock<TOutput> output, int estimatedOutputCount)
        {
            Start = WrapStart(start, output);
            Output = output;
            EstimatedOutputCount = estimatedOutputCount;
            Completion = output.Completion;
        }

        public StartableBlock(Action start)
        {
            var output = new BufferBlock<TOutput>();

            Start = WrapStart(() =>
                                  {
                                      start();
                                      output.Complete();
                                  },
                              output);
            Output = output;
            Completion = output.Completion;
        }

        public StartableBlock(StartableBlock<TOutput> start, ProcessingBlock<TOutput> output, Task completion)
        {
            Start = start.Start;
            Output = output.Processor;
            EstimatedOutputCount = start.EstimatedOutputCount;
            Completion = completion;
        }

        private Action WrapStart(Action start, ISourceBlock<TOutput> output)
        {
            return () =>
                       {
                           try
                           {
                               start();
                           }
                           catch (Exception ex)
                           {
                               output.Fault(ex);
                           }
                       };
        }
    }

    public static class StartableBlockExtensions
    {
        public static void ContinueWith<TOutput, TContinuationOutput>(this StartableBlock<TOutput> block, StartableBlock<TContinuationOutput> continuationBlock)
        {
            block.Output.IgnoreOutput();

            block.Completion.ContinueWith(x =>
                                              {
                                                  if (x.IsFaulted)
                                                  {
                                                      continuationBlock.Output.Fault(x.Exception);
                                                  }
                                                  else
                                                  {
                                                      try
                                                      {
                                                          continuationBlock.Start();
                                                      }
                                                      catch (Exception ex)
                                                      {
                                                          continuationBlock.Output.Fault(ex);
                                                      }
                                                  }
                                              });
        }
    }
}
