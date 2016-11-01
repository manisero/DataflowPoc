using System.Threading;
using System.Threading.Tasks.Dataflow;
using Dataflow.Logic;
using Dataflow.Models;

namespace Dataflow.Pipelines.BlockFactories
{
    public class ComputePersonFieldsBlockFactory
    {
        private readonly PersonFieldsComputer _personFieldsComputer;

        public ComputePersonFieldsBlockFactory(PersonFieldsComputer personFieldsComputer)
        {
            _personFieldsComputer = personFieldsComputer;
        }

        public ProcessingBlock<Data> Create(int estimatedInputCount, CancellationToken cancellation)
        {
            var computeBlock = new TransformBlock<Data, Data>(x =>
                                                                  {
                                                                      if (x.IsValid)
                                                                      {
                                                                          _personFieldsComputer.Compute(x);
                                                                      }

                                                                      return x;
                                                                  },
                                                              new ExecutionDataflowBlockOptions { CancellationToken = cancellation, BoundedCapacity = 1 });

            return new ProcessingBlock<Data>
                {
                    Processor = computeBlock,
                    EstimatedOutputCount = estimatedInputCount,
                    Completion = computeBlock.Completion
                };
        }
    }
}
