using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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
