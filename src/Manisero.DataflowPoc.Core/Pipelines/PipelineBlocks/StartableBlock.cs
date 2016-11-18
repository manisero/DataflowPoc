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
        public static StartableBlock<TOutput> ContinueWith<TOutput>(this StartableBlock<TOutput> block, Action<Task> continuationAction, bool executeOnFault = true)
        {
            return new StartableBlock<TOutput>
                {
                    Start = block.Start,
                    Output = block.Output,
                    EstimatedOutputCount = block.EstimatedOutputCount,
                    Completion = block.Completion.ContinueWithStatusPropagation(continuationAction, executeOnFault)
                };
        }

        public static void ContinueWith<TOutput>(this Task task, StartableBlock<TOutput> continuationBlock)
        {
            task.ContinueWith(x =>
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
