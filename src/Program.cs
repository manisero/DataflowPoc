using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Dataflow.Extensions;
using Dataflow.Logic;
using Dataflow.Models;
using Dataflow.PipelineConstruction;

namespace Dataflow
{
    class Program
    {
        private const bool THROW = false;

        private const string PEOPLE_JSON_FILE_PATH = @"\\VBOXSVR\temp\people.json";
        private const string PEOPLE_RESULT_FILE_PATH = @"\\VBOXSVR\temp\people_result.txt";

        static void Main(string[] args)
        {
            var readingBlockFactory = new ReadingBlockFactory(true, new FileLinesCounter(), new DataReader(), new StreamLinesReader(), new DataParser());
            var writingBlockFactory = new WritingBlockFactory(new DataWriter());
            var throwingBlockFactory = new ThrowingBlockFactory();
            var emptyBlockFactory = new EmptyBlockFactory();

            var cancellationSource = new CancellationTokenSource();
            var stopwatch = new Stopwatch();
            var duration = default(TimeSpan);

            // Create blocks
            // TODO: Progress reporting approach 1: before anything
            var readBlock = readingBlockFactory.Create(PEOPLE_JSON_FILE_PATH, cancellationSource.Token);
            var processBlock = writingBlockFactory.Create(PEOPLE_RESULT_FILE_PATH, cancellationSource.Token);
            var throwBlock = THROW ? throwingBlockFactory.Create<Data>(cancellationSource.Token) : emptyBlockFactory.Create<Data>(cancellationSource.Token);
            // TODO: Data-level error handling (reporting / logging)
            // TODO: Progress reporting approach 2: after everything
            var terminateBlock = DataflowBlock.NullTarget<Data>();  // Never completes

            // Link blocks
            readBlock.Output.LinkWithCompletion(processBlock.Processor);
            processBlock.Processor.LinkWithCompletion(throwBlock.Processor);
            throwBlock.Processor.LinkWithCompletion(terminateBlock);

            // Handle completion
            var completion = CreateCompletion(cancellationSource,
                                              readBlock.Completion,
                                              processBlock.Completion,
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
            readBlock.Start();

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

        private static Task CreateCompletion(CancellationTokenSource cancellationSource, params Task[] tasks)
        {
            var faultHandlers = tasks.Select(x => x.ContinueWithStatusPropagation(t =>
                                                                                      {
                                                                                          if (t.IsFaulted && !cancellationSource.IsCancellationRequested)
                                                                                          {
                                                                                              cancellationSource.Cancel();
                                                                                          }
                                                                                      }));

            return Task.WhenAll(faultHandlers);
        }
    }
}
