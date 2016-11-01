using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Dataflow.Extensions;

namespace Dataflow.Pipelines
{
    public class StartableBlock<TOutput>
    {
        public Action Start { get; set; }

        public ISourceBlock<TOutput> Output { get; set; }

        public int EstimatedOutputCount { get; set; }

        public Task Completion { get; set; }
    }

    public class ProcessingBlock<TData>
    {
        public IPropagatorBlock<TData, TData> Processor { get; set; }

        public Task Completion { get; set; }

        public static ProcessingBlock<TData> Create(Action<TData> process, CancellationToken cancellation)
        {
            return Create(x =>
                              {
                                  process(x);
                                  return x;
                              },
                          cancellation);
        }

        public static ProcessingBlock<TData> Create(Func<TData, TData> process, CancellationToken cancellation)
        {
            var processor = DataflowFacade.TransformBlock<TData>(process, cancellation);

            return new ProcessingBlock<TData>
                {
                    Processor = processor,
                    Completion = processor.Completion
                };
        }
    }
}
