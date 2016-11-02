using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Dataflow.Extensions;

namespace Dataflow.Pipelines.PipelineBlocks
{
    public class ProcessingBlock<TData>
    {
        public IPropagatorBlock<TData, TData> Processor { get; set; }

        public Task Completion { get; set; }

        public static ProcessingBlock<TData> Create(string name, Action<TData> process, CancellationToken cancellation)
        {
            return Create(
                name,
                x =>
                    {
                        process(x);
                        return x;
                    },
                cancellation);
        }

        public static ProcessingBlock<TData> Create(string name, Func<TData, TData> process, CancellationToken cancellation)
        {
            var processor = DataflowFacade.TransformBlock<TData>(name, process, cancellation);

            return new ProcessingBlock<TData>
                {
                    Processor = processor,
                    Completion = processor.Completion
                };
        }
    }
}