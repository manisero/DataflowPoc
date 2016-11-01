using System.Threading;
using System.Threading.Tasks.Dataflow;
using Dataflow.Logic;
using Dataflow.Models;

namespace Dataflow.Pipelines.BlockFactories
{
    public class ValidatePersonBlockFactory
    {
        private readonly PersonValidator _personValidator;

        public ValidatePersonBlockFactory(PersonValidator personValidator)
        {
            _personValidator = personValidator;
        }

        public ProcessingBlock<Data> Create(int estimatedInputCount, CancellationToken cancellation)
        {
            var validateBlock = new TransformBlock<Data, Data>(
                x =>
                    {
                        if (x.IsValid)
                        {
                            _personValidator.Validate(x);
                        }

                        return x;
                    },
                new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            return new ProcessingBlock<Data>
                {
                    Processor = validateBlock,
                    EstimatedOutputCount = estimatedInputCount,
                    Completion = validateBlock.Completion
                };
        }
    }
}
