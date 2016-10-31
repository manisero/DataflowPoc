using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

        public int EstimatedOutputCount { get; set; }

        public Task Completion { get; set; }
    }
}
