using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Dataflow.Extensions;
using Dataflow.Models;

namespace Dataflow.Pipelines.BlockFactories
{
    public class ProgressReportingBlockFactory
    {
        private class State
        {
            public int ItemsProcessed { get; set; }
            public bool Reported100 { get; set; } 
        }

        public ProcessingBlock<TData> Create<TData>(IProgress<PipelineProgress> progress, int estimatedInputCount, CancellationToken cancellation)
        {
            var batchSize = Settings.ProgressBatchSize;
            var state = new State();
            
            // Create blocks
            var reportBlock = new TransformBlock<TData, TData>(
                x =>
                    {
                        TryReport(state, batchSize, estimatedInputCount, progress);
                        return x;
                    },
                new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            // Handle completion
            var completion = reportBlock.Completion.ContinueWithStatusPropagation(
                x =>
                    {
                        if (!x.IsFaulted && !x.IsCanceled && !state.Reported100)
                        {
                            progress.Report(new PipelineProgress { Percentage = 100 });
                        }
                    });

            return new ProcessingBlock<TData>
                {
                    Processor = reportBlock,
                    Completion = completion
                };
        }

        private void TryReport(State state, int batchSize, int estimatedItemsCount, IProgress<PipelineProgress> progress)
        {
            if (state.Reported100)
            {
                return;
            }

            state.ItemsProcessed++;

            if (state.ItemsProcessed >= estimatedItemsCount)
            {
                Report(state, 100, progress);
            }
            else if (state.ItemsProcessed % batchSize == 0)
            {
                var percentage = state.ItemsProcessed.PercentageOf(estimatedItemsCount);
                Report(state, percentage, progress);
            }
        }

        private void Report(State state, byte percentage, IProgress<PipelineProgress> progress)
        {
            if (percentage >= 100)
            {
                percentage = 100;
                state.Reported100 = true;
            }

            progress.Report(new PipelineProgress { Percentage = percentage });
        }
    }
}
