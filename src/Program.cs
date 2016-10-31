using System;
using System.Diagnostics;
using System.Threading;
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
            throwBlock.Processor.IgnoreOutput();

            // Handle completion
            var completion = TaskExtensions.CreateCommonCompletion(cancellationSource,
                                                                   peopleStreamBlock.Completion,
                                                                   throwBlock.Completion).ContinueWith(
                                                                       x =>
                                                                           {
                                                                               duration = stopwatch.Elapsed;

                                                                               if (x.IsFaulted)
                                                                               {
                                                                                   // TODO: Handle error
                                                                               }
                                                                               else if (x.IsCanceled)
                                                                               {
                                                                                   Console.WriteLine("Cancelled.");
                                                                               }
                                                                               else
                                                                               {
                                                                                   Console.WriteLine("Complete.");
                                                                               }

                                                                               Console.WriteLine($"Took {duration.TotalMilliseconds}ms.");
                                                                           });
           
            // Start
            stopwatch.Start();
            peopleStreamBlock.Start();

            var input = (char)Console.Read();

            if (input == 'c')
            {
                if (!completion.IsCompleted) // IsCompleted is true even in case of exception
                {
                    cancellationSource.Cancel();
                }
            }

            try
            {
                completion.Wait();
            }
            catch (Exception e)
            {
                var stackTrace = e.StackTrace;
            }

            cancellationSource.Dispose();
        }
    }
}
