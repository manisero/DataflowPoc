using System;
using System.Threading.Tasks;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Pipelines.PipelineBlocks;

namespace Manisero.DataflowPoc.Pipelines
{
    public class PipelineExecutor
    {
        public async Task<PipelineExecutionResult> Execute<TData>(StartableBlock<TData> pipeline)
        {
            var pipelineExecutionResult = new PipelineExecutionResult();

            // Ignore pipeline output
            pipeline.Output.IgnoreOutput();

            // Handle completion
            var completionHandler = pipeline.Completion.ContinueWith(
                (completion, executionResult) => FillExecutionResult(completion, (PipelineExecutionResult)executionResult),
                pipelineExecutionResult);

            // Execute
            pipelineExecutionResult.StartTs = DateTime.UtcNow;
            pipeline.Start();

            return await completionHandler;
        }

        private PipelineExecutionResult FillExecutionResult(Task pipelineCompletion, PipelineExecutionResult executionResult)
        {
            executionResult.FinishTs = DateTime.UtcNow;

            if (pipelineCompletion.IsFaulted)
            {
                executionResult.Faulted = true;
                executionResult.Exception = pipelineCompletion.Exception;
            }
            else if (pipelineCompletion.IsCanceled)
            {
                executionResult.Canceled = true;
            }

            return executionResult;
        }
    }
}
