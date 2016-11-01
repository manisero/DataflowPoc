using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dataflow.Extensions;
using Dataflow.Models;

namespace Dataflow.Pipelines
{
    public class PipelineFactory
    {
        public StartableBlock<Data> Create(StartableBlock<Data> source, ProcessingBlock<Data>[] processors, CancellationTokenSource cancellationSource)
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

            return new StartableBlock<Data>
                {
                    Start = source.Start,
                    Output = outputBlock.Processor,
                    EstimatedOutputCount = outputBlock.EstimatedOutputCount,
                    Completion = completion
                };
        }
    }
}
