using System.Threading;
using System.Threading.Tasks.Dataflow;
using Dataflow.Models;

namespace Dataflow.Pipelines
{
    public class HandleErrorBlockFactory
    {
        public ProcessingBlock<Data> Create(CancellationToken cancellation)
        {
            // TODO: Implement error handling
            var handleErrorBlock = new TransformBlock<Data, Data>(x => x, new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            return new ProcessingBlock<Data>
                {
                    Processor = handleErrorBlock,
                    Completion = handleErrorBlock.Completion
                };
        }
    }
}
