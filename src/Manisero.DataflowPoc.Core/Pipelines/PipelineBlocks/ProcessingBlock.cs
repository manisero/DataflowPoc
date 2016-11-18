using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks
{
    public class ProcessingBlock<TData>
    {
        public IPropagatorBlock<TData, TData> Processor { get; set; }
        public Task Completion { get; set; }

        public ProcessingBlock(IPropagatorBlock<TData, TData> processor, Task customCompletion = null)
        {
            Processor = processor;
            Completion = customCompletion ?? processor.Completion;
        }
    }
}
