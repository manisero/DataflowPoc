using System;
using System.Threading;
using System.Threading.Tasks;
using Dataflow.Logic;
using Dataflow.Pipelines;
using Dataflow.Pipelines.GenericBlockFactories;
using Dataflow.Pipelines.PeopleStream;
using Dataflow.Pipelines.PeopleStream.BlockFactories;

namespace Dataflow
{
    class Program
    {
        static void Main(string[] args)
        {
            // Synchronous
            var synchronousPeopleProcessor = new SynchronousPeopleProcessor(new FileLinesCounter(),
                                                                            new DataReader(),
                                                                            new PersonValidator(),
                                                                            new PersonFieldsComputer(),
                                                                            new DataWriter());

            var synchronousDuration = synchronousPeopleProcessor.Process(Settings.PeopleJsonFilePath,
                                                                         Settings.PeopleTargetFilePath,
                                                                         Settings.ErrorsFilePath);

            Console.WriteLine($"Synchronous took {synchronousDuration.TotalMilliseconds}ms.");

            // Dataflow
            var peoplePipelineFactory = new PeoplePipelineFactory(new ReadingBlockFactory(new FileLinesCounter(),
                                                                                          new DataReader(),
                                                                                          new StreamLinesReader(),
                                                                                          new DataParser()),
                                                                  new PersonValidator(),
                                                                  new PersonFieldsComputer(),
                                                                  new WritingBlockFactory(new DataWriter()),
                                                                  new ThrowingBlockFactory(),
                                                                  new EmptyBlockFactory(),
                                                                  new ProgressReportingBlockFactory(),
                                                                  new PipelineFactory());
            var pipelineExecutor = new PipelineExecutor();

            using (var cancellationSource = new CancellationTokenSource())
            {
                var progress = new Progress<PipelineProgress>(x => Console.WriteLine($"{x.Percentage}% processed."));

                var pipeline = peoplePipelineFactory.Create(Settings.PeopleJsonFilePath,
                                                            Settings.PeopleTargetFilePath,
                                                            Settings.ErrorsFilePath,
                                                            progress,
                                                            cancellationSource);

                Task.Run(() => WaitForCancellation(cancellationSource));

                var executionResult = pipelineExecutor.Execute(pipeline).Result;
                HandleExecutionResult(executionResult);
            }
        }

        private static void WaitForCancellation(CancellationTokenSource cancellationSource)
        {
            var input = (char)Console.Read();

            if (input == 'c')
            {
                cancellationSource.Cancel();
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
