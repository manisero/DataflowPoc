using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Dataflow.Extensions;

namespace Dataflow.Pipelines
{
    public class PipelineFactory
    {
        public StartableBlock<TData> Create<TData>(CancellationTokenSource cancellationSource,
                                                   StartableBlock<TData> source,
                                                   ProcessingBlock<TData>[] processors,
                                                   ProcessingBlock<TData> errorHandler,
                                                   ProcessingBlock<TData> progressReporter,
                                                   Predicate<TData> validDataPredicate)
        {
            // Link blocks
            source.Output.LinkWithCompletion(processors[0].Processor, validDataPredicate);
            source.Output.LinkTo(errorHandler.Processor, x => !validDataPredicate(x));

            for (var i = 0; i < processors.Length - 1; i++)
            {
                processors[i].Processor.LinkWithCompletion(processors[i + 1].Processor, validDataPredicate);
                processors[i].Processor.LinkTo(errorHandler.Processor, x => !validDataPredicate(x));
            }

            var lastProcessor = processors.Last();
            lastProcessor.Processor.LinkTo(progressReporter.Processor, validDataPredicate);
            lastProcessor.Processor.LinkTo(errorHandler.Processor, x => !validDataPredicate(x));

            errorHandler.Processor.LinkTo(progressReporter.Processor);

            // Propagate completions of multiple inputs
            Task.WhenAll(new[] { source.Output.Completion }.Concat(processors.Select(x => x.Processor.Completion)))
                .ContinueWith(x =>
                                  {
                                      // TODO: Handle fault and cancellation
                                      errorHandler.Processor.Complete();
                                  }); // TODO: Consider passing cancellationSource.Token

            Task.WhenAll(lastProcessor.Processor.Completion, errorHandler.Processor.Completion)
                .ContinueWith(x =>
                                  {
                                      // TODO: Handle fault and cancellation
                                      progressReporter.Processor.Complete();
                                  }); // TODO: Consider passing cancellationSource.Token

            // Create global completion
            var completion = Extensions.TaskExtensions.CreateGlobalCompletion(new[] { source.Completion }.Concat(processors.Select(x => x.Completion))
                                                                                                         .Concat(new[] { errorHandler.Completion, progressReporter.Completion }),
                                                                              cancellationSource);

            // Create pipeline
            var output = progressReporter;

            return new StartableBlock<TData>
                {
                    Start = source.Start,
                    Output = output.Processor,
                    EstimatedOutputCount = output.EstimatedOutputCount,
                    Completion = completion
                };
        }
    }
}
