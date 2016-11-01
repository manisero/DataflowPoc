using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dataflow.Extensions;

namespace Dataflow.Pipelines
{
    public class PipelineFactory
    {
        public StartableBlock<TData> Create<TData>(StartableBlock<TData> source, ProcessingBlock<TData>[] processors, CancellationTokenSource cancellationSource)
        {
            // Link blocks
            source.Output.LinkWithCompletion(processors[0].Processor);

            for (var i = 0; i < processors.Length - 1; i++)
            {
                processors[i].Processor.LinkWithCompletion(processors[i + 1].Processor);
            }

            // Handle completion
            var completions = new List<Task> { source.Completion };
            completions.AddRange(processors.Select(x => x.Completion));

            var completion = Extensions.TaskExtensions.CreateGlobalCompletion(completions, cancellationSource);

            // Create pipeline
            var outputBlock = processors.Last();

            return new StartableBlock<TData>
                {
                    Start = source.Start,
                    Output = outputBlock.Processor,
                    EstimatedOutputCount = outputBlock.EstimatedOutputCount,
                    Completion = completion
                };
        }
    }
}
