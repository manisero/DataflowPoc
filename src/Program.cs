using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dataflow.Extensions;
using Dataflow.Logic;
using Dataflow.Models;
using Dataflow.Pipelines;
using Dataflow.Pipelines.PeopleStream;

namespace Dataflow
{
    class Program
    {
        private const bool THROW = false;

        private const string PEOPLE_JSON_FILE_PATH = @"\\VBOXSVR\temp\people.json";
        private const string PEOPLE_RESULT_FILE_PATH = @"\\VBOXSVR\temp\people_result.txt";

        static void Main(string[] args)
        {
            var peopleStreamFactory = new PeopleStreamFactory(new ReadingBlockFactory(true, new FileLinesCounter(), new DataReader(), new StreamLinesReader(), new DataParser()),
                                                              new WritingBlockFactory(new DataWriter()));
            var throwingBlockFactory = new ThrowingBlockFactory();
            var emptyBlockFactory = new EmptyBlockFactory();
            var pipelineExecutor = new PipelineExecutor();

            var cancellationSource = new CancellationTokenSource();
            var stopwatch = new Stopwatch();
            var duration = default(TimeSpan);

            // Create blocks
            // TODO: Progress reporting approach 1: before anything
            var peopleStreamBlock = peopleStreamFactory.Create(PEOPLE_JSON_FILE_PATH, PEOPLE_RESULT_FILE_PATH, cancellationSource);
            var throwBlock = THROW ? throwingBlockFactory.Create<Data>(cancellationSource.Token) : emptyBlockFactory.Create<Data>(cancellationSource.Token);
            // TODO: Data-level error handling (reporting / logging)
            // TODO: Progress reporting approach 2: after everything

            // Link blocks
            peopleStreamBlock.Output.LinkWithCompletion(throwBlock.Processor);

            // Handle completion
            var completion = Extensions.TaskExtensions.CreateGlobalCompletion(cancellationSource,
                                                                              peopleStreamBlock.Completion,
                                                                              throwBlock.Completion);
           
            // Start
            var pipeline = new StartableBlock<Data>
                {
                    Start = peopleStreamBlock.Start,
                    Output = throwBlock.Processor,
                    EstimatedOutputCount = throwBlock.EstimatedOutputCount,
                    Completion = completion
                };

            var pipelineCompletion = pipelineExecutor.Execute(pipeline);

            WaitForCancellation(pipelineCompletion, cancellationSource);

            try
            {
                pipelineCompletion.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            cancellationSource.Dispose();
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
    }
}
