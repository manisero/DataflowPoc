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
        private const bool THROW_TEST = true;
        private const bool OPTIMIZE_READING = false;

        private const string PEOPLE_JSON_FILE_PATH = @"\\VBOXSVR\temp\people.json";
        private const string PEOPLE_RESULT_FILE_PATH = @"\\VBOXSVR\temp\people_result.txt";

        static void Main(string[] args)
        {
            var peoplePipelineFactory = new PeoplePipelineFactory(THROW_TEST,
                                                                  new ReadingBlockFactory(OPTIMIZE_READING,
                                                                                          new FileLinesCounter(),
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
                var pipeline = peoplePipelineFactory.Create(PEOPLE_JSON_FILE_PATH, PEOPLE_RESULT_FILE_PATH, cancellationSource);
                var pipelineCompletion = pipelineExecutor.Execute(pipeline);

                //WaitForCancellation(pipelineCompletion, cancellationSource);

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
