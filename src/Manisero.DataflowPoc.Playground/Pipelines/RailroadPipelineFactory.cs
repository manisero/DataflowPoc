using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Pipelines.PipelineBlocks;

namespace Manisero.DataflowPoc.Pipelines
{
    public class RailroadPipelineFactory
    {
        public StartableBlock<TData> Create<TData>(StartableBlock<TData> source,
                                                   ProcessingBlock<TData>[] processors,
                                                   ProcessingBlock<TData> errorHandler,
                                                   ProcessingBlock<TData> output,
                                                   Predicate<TData> validDataPredicate,
                                                   CancellationTokenSource cancellationSource)
        {
            // The pipeline looks like this:
            // source -> processor1 -> processor2 -> output
            //       \      |          |             ^
            //        \     v          v            /
            //         \--> errorHandler ----------/

            // Link blocks
            source.Output.LinkWithCompletion(processors[0].Processor, validDataPredicate);
            source.Output.LinkTo(errorHandler.Processor, x => !validDataPredicate(x));

            for (var i = 0; i < processors.Length - 1; i++)
            {
                processors[i].Processor.LinkWithCompletion(processors[i + 1].Processor, validDataPredicate);
                processors[i].Processor.LinkTo(errorHandler.Processor, x => !validDataPredicate(x));
            }

            var lastProcessor = processors.Last();
            lastProcessor.Processor.LinkTo(output.Processor, validDataPredicate);
            lastProcessor.Processor.LinkTo(errorHandler.Processor, x => !validDataPredicate(x));

            errorHandler.Processor.LinkTo(output.Processor);

            // Propagate completions of multiple inputs
            errorHandler.Processor.DeriveCompletionOrFaultFrom(new[] { source.Output }.Concat(processors.Select(x => x.Processor)));
            output.Processor.DeriveCompletionOrFaultFrom(lastProcessor.Processor, errorHandler.Processor);

            // Create global completion
            var completion = TaskExtensions.CreateGlobalCompletion(new[] { source.Completion }.Concat(processors.Select(x => x.Completion))
                                                                                              .Concat(new[] { errorHandler.Completion, output.Completion }),
                                                                   cancellationSource);

            return new StartableBlock<TData>
                {
                    Start = source.Start,
                    Output = output.Processor,
                    EstimatedOutputCount = source.EstimatedOutputCount,
                    Completion = completion
                };
        }
    }
}
