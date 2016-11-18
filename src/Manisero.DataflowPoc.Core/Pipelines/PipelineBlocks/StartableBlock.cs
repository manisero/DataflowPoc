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

        public StartableBlock(Action start, ISourceBlock<TOutput> output, int estimatedOutputCount, Task customCompletion = null, bool isComposite = false)
        {
            Start = isComposite ? start : WrapStart(start, output);
            Output = output;
            EstimatedOutputCount = estimatedOutputCount;
            Completion = customCompletion ?? output.Completion;
        }

        public StartableBlock(StartableBlock<TOutput> source, ProcessingBlock<TOutput> output, Task completion)
            : this(source.Start, output.Processor, source.EstimatedOutputCount, completion, true)
        {
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

        public void ContinueWith<TContinuationOutput>(StartableBlock<TContinuationOutput> continuationBlock)
        {
            Output.IgnoreOutput();

            Completion.ContinueWith(x =>
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
