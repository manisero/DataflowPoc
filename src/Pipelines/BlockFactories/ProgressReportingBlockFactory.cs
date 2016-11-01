using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Dataflow.Extensions;
using Dataflow.Models;

namespace Dataflow.Pipelines.BlockFactories
{
    public class ProgressReportingBlockFactory
    {
        public ProcessingBlock<TData> Create<TData>(IProgress<PipelineProgress> progress, int estimatedInputCount, CancellationToken cancellation)
        {
            var batchSize = Settings.ProgressBatchSize;
            var itemsProcessed = 0;
            
            // Create blocks
            var reportBlock = new TransformBlock<TData, TData>(x =>
                                                                   {
                                                                       itemsProcessed++;

                                                                       if (itemsProcessed % batchSize == 0)
                                                                       {
                                                                           var percentage = itemsProcessed.PercentageOf(estimatedInputCount);
                                                                           progress.Report(new PipelineProgress { Percentage = percentage });
                                                                       }

                                                                       return x;
                                                                   },
                                                               new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            // Handle completion
            var completion = reportBlock.Completion.ContinueWithStatusPropagation(
                x =>
                    {
                        if (!x.IsFaulted && !x.IsCanceled)
                        {
                            progress.Report(new PipelineProgress { Percentage = 100 });
                        }
                    });

            return new ProcessingBlock<TData>
                {
                    Processor = reportBlock,
                    EstimatedOutputCount = estimatedInputCount,
                    Completion = completion
                };
        }
    }
}
