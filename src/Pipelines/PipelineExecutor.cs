using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dataflow.Extensions;
using Dataflow.Models;

namespace Dataflow.Pipelines
{
    public class PipelineExecutor
    {
        public async Task Execute(StartableBlock<Data> pipeline)
        {
            var stopwatch = new Stopwatch();
            var duration = default(TimeSpan);

            pipeline.Output.IgnoreOutput();

            // Handle completion
            var completionHandler = pipeline.Completion.ContinueWith(
                x =>
                {
                    duration = stopwatch.Elapsed;

                    if (x.IsFaulted)
                    {
                        // TODO: Handle error
                    }
                    else if (x.IsCanceled)
                    {
                        Console.WriteLine("Canceled.");
                    }
                    else
                    {
                        Console.WriteLine("Complete.");
                    }

                    Console.WriteLine($"Took {duration.TotalMilliseconds}ms.");
                });

            // Start
            stopwatch.Start();
            pipeline.Start();

            await completionHandler;
        }
    }
}
