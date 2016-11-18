using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Manisero.DataflowPoc.Core.Extensions;

namespace Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks
{
    public class StartableBlock<TOutput>
    {
        public Action Start { get; }
        public ISourceBlock<TOutput> Output { get; }
        public int EstimatedOutputCount { get; }
        public Task Completion { get; private set; }

        public StartableBlock(Action start, ISourceBlock<TOutput> output, int estimatedOutputCount, Task customCompletion = null)
        {
            Start = WrapStart(start, output);
            Output = output;
            EstimatedOutputCount = estimatedOutputCount;
            Completion = customCompletion ?? output.Completion;
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

        public void ContinueCompletionWith(Action<Task> continuationAction)
        {
            Completion = Completion.ContinueWithStatusPropagation(continuationAction);
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
