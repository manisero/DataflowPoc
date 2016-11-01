using System;
using System.Threading;
using System.Threading.Tasks;
using Dataflow.Logic;
using Dataflow.Models;
using Dataflow.Pipelines;
using Dataflow.Pipelines.BlockFactories;

namespace Dataflow
{
    class Program
    {
        static void Main(string[] args)
        {
            var peoplePipelineFactory = new PeoplePipelineFactory(new ReadingBlockFactory(new FileLinesCounter(),
                                                                                          new DataReader(),
                                                                                          new StreamLinesReader(),
                                                                                          new DataParser()),
                                                                  new WritingBlockFactory(new DataWriter()),
                                                                  new ThrowingBlockFactory(),
                                                                  new EmptyBlockFactory(),
                                                                  new PipelineFactory());
            var pipelineExecutor = new PipelineExecutor();

            using (var cancellationSource = new CancellationTokenSource())
            {
                var pipeline = peoplePipelineFactory.Create(Settings.PeopleJsonFilePath, Settings.PeopleTargetFilePath, cancellationSource);
                var pipelineCompletion = pipelineExecutor.Execute(pipeline);

                Task.Run(() => WaitForCancellation(pipelineCompletion, cancellationSource));

                var executionResult = pipelineCompletion.Result;
                HandleExecutionResult(executionResult);
            }
        }

        private static void WaitForCancellation(Task completion, CancellationTokenSource cancellationSource)
        {
            var input = (char)Console.Read();

            if (input == 'c')
            {
                if (!completion.IsCompleted) // IsCompleted is true even in case of exception
                {
                    cancellationSource.Cancel();
                }
            }
        }

        private static void HandleExecutionResult(PipelineExecutionResult executionResult)
        {
            var duration = executionResult.FinishTs - executionResult.StartTs;
            Console.WriteLine($"Took {duration.TotalMilliseconds}ms.");

            if (executionResult.Faulted)
            {
                Console.WriteLine("Faulted. Exception:");
                Console.WriteLine(executionResult.Exception);
            }
            else if (executionResult.Canceled)
            {
                Console.WriteLine("Canceled.");
            }
            else
            {
                Console.WriteLine("Complete.");
            }
        }
    }
}
