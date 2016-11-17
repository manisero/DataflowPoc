using System.Linq;
using System.Threading;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;

namespace Manisero.DataflowPoc.Core.Pipelines
{
    public interface IStraightPipelineFactory
    {
        StartableBlock<TData> Create<TData>(StartableBlock<TData> source, ProcessingBlock<TData>[] processors, CancellationToken cancellation);
    }

    public class StraightPipelineFactory : IStraightPipelineFactory
    {
        public StartableBlock<TData> Create<TData>(StartableBlock<TData> source, ProcessingBlock<TData>[] processors, CancellationToken cancellation)
        {
            // The pipeline looks like this:
            // source -> processor1 -> processor2 -> processor3 (output)

            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            // Link blocks
            source.Output.LinkWithCompletion(processors[0].Processor);

            for (var i = 0; i < processors.Length - 1; i++)
            {
                processors[i].Processor.LinkWithCompletion(processors[i + 1].Processor);
            }

            // Create completion
            var globalCompletion = TaskExtensions.CreateGlobalCompletion(new[] { source.Completion }.Concat(processors.Select(x => x.Completion)),
                                                                         cancellationSource);

            var completion = globalCompletion.ContinueWithStatusPropagation(_ => cancellationSource.Dispose());

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
