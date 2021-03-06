using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Manisero.DataflowPoc.Extensions;

namespace Manisero.DataflowPoc.Pipelines.PipelineBlocks
{
    public class ProcessingBlock<TData>
    {
        public IPropagatorBlock<TData, TData> Processor { get; set; }

        public Task Completion { get; set; }

        public static ProcessingBlock<TData> Create(string name,
                                                    Func<TData, int> dataIdGetter,
                                                    Action<TData> process,
                                                    CancellationToken cancellation,
                                                    int maxDegreeOfParallelism = 1)
        {
            return Create(
                name,
                dataIdGetter,
                x =>
                    {
                        process(x);
                        return x;
                    },
                cancellation,
                maxDegreeOfParallelism);
        }

        public static ProcessingBlock<TData> Create(string name,
                                                    Func<TData, int> dataIdGetter,
                                                    Func<TData, TData> process,
                                                    CancellationToken cancellation,
                                                    int maxDegreeOfParallelism = 1)
        {
            var processor = DataflowFacade.TransformBlock<TData>(name, dataIdGetter, process, cancellation, maxDegreeOfParallelism);

            return new ProcessingBlock<TData>
                {
                    Processor = processor,
                    Completion = processor.Completion
                };
        }
    }
}