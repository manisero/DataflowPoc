using System.Linq;
using System.Threading;
using Dataflow.Extensions;
using Dataflow.Pipelines.PipelineBlocks;

namespace Dataflow.Pipelines
{
    public class StraightPipelineFactory
    {
        public StartableBlock<TData> Create<TData>(StartableBlock<TData> source,
                                                   ProcessingBlock<TData>[] processors,
                                                   CancellationTokenSource cancellationSource)
        {
            // The pipeline looks like this:
            // source -> processor1 -> processor2 -> processor3 (output)

            // Link blocks
            source.Output.LinkWithCompletion(processors[0].Processor);

            for (var i = 0; i < processors.Length - 1; i++)
            {
                processors[i].Processor.LinkWithCompletion(processors[i + 1].Processor);
            }

            // Create global completion
            var completion = TaskExtensions.CreateGlobalCompletion(new[] { source.Completion }.Concat(processors.Select(x => x.Completion)),
                                                                   cancellationSource);

            return new StartableBlock<TData>
                {
                    Start = source.Start,
                    Output = processors.Last().Processor,
                    EstimatedOutputCount = source.EstimatedOutputCount,
                    Completion = completion
                };
        }
    }
}
