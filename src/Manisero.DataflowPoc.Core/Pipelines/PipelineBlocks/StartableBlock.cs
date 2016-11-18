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
        public static StartableBlock<object> CreateStartOnlyBlock(Action start) => CreateStartOnlyBlock<object>(start);

        public static StartableBlock<TOutput> CreateStartOnlyBlock<TOutput>(Action start)
        {
            var output = new BufferBlock<TOutput>();

            return new StartableBlock<TOutput>
                {
                    Start = () =>
                                {
                                    try
                                    {
                                        start();
                                        output.Complete();
                                    }
                                    catch (Exception ex)
                                    {
                                        ((ISourceBlock<TOutput>)output).Fault(ex);
                                    }
                                },
                    Output = output,
                    Completion = output.Completion
                };
        }

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
