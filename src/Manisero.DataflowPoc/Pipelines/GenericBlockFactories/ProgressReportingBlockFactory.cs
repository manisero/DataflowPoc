using System;
using System.Threading;
using Manisero.DataflowPoc.Extensions;
using Manisero.DataflowPoc.Pipelines.PipelineBlocks;

namespace Manisero.DataflowPoc.Pipelines.GenericBlockFactories
{
    public class ProgressReportingBlockFactory
    {
        private class State
        {
            public int ItemsProcessed { get; set; }
            public bool Reported100 { get; set; } 
        }

        public ProcessingBlock<TData> Create<TData>(Func<TData, int> dataIdGetter,
                                                    IProgress<PipelineProgress> progress,
                                                    int estimatedInputCount,
                                                    int inputPerReport,
                                                    CancellationToken cancellation)
        {
            var state = new State();

            // Create blocks
            var reportBlock = DataflowFacade.TransformBlock(
                "ReportProgress",
                dataIdGetter,
                x => TryReport(state, inputPerReport, estimatedInputCount, progress),
                cancellation);

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

        private void TryReport(State state, int inputPerReport, int estimatedItemsCount, IProgress<PipelineProgress> progress)
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
            else if (state.ItemsProcessed % inputPerReport == 0)
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
