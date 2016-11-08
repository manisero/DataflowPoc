using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Manisero.DataflowPoc.Core.Pipelines;
using Manisero.DataflowPoc.DataExporter.Logic;
using Manisero.DataflowPoc.DataExporter.Pipeline;
using Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories;

namespace Manisero.DataflowPoc.DataExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DataExporter"].ConnectionString;

            var sqlConnectionResolver = new SqlConnectionResolver(connectionString);
            var pipelineFactory = new PipelineFactory(new ReadBlockFactory(new PeopleCounter(sqlConnectionResolver),
                                                                           new PeopleBatchReader(sqlConnectionResolver)),
                                                      new WriteBlockFactory(new PeopleBatchWriter()),
                                                      new StraightPipelineFactory());
            var pipelineExecutor = new PipelineExecutor();

            using (var cancellation = new CancellationTokenSource())
            {
                Task.Run(() => WaitForCancellation(cancellation));

                var progress = new Progress<PipelineProgress>(x => Console.WriteLine($"{x.Percentage}% processed."));
                var pipeline = pipelineFactory.Create(ConfigurationManager.AppSettings["PeopleTargetFilePath"], cancellation);
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
