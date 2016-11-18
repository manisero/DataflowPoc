﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines;
using Manisero.DataflowPoc.Core.Pipelines.GenericBlockFactories;
using Manisero.DataflowPoc.DataExporter.Logic;
using Manisero.DataflowPoc.DataExporter.Pipeline;
using Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories;

namespace Manisero.DataflowPoc.DataExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var sqlConnectionResolver = new SqlConnectionResolver(Settings.ConnectionString);
            var pipelineFactory = new PipelineFactory(new ReadSummaryBlockFactory(new PeopleSummaryReader(sqlConnectionResolver)),
                                                      new ReadPeopleBlockFactory(new PeopleCounter(sqlConnectionResolver),
                                                                                 new PeopleBatchReader(sqlConnectionResolver)),
                                                      new WriteCsvBlockFactory(),
                                                      new ProgressReportingBlockFactory(),
                                                      new StraightPipelineFactory());
            var pipelineExecutor = new PipelineExecutor();

            using (var cancellation = new CancellationTokenSource())
            {
                Task.Run(() => WaitForCancellation(cancellation));

                var progress = new Progress<PipelineProgress>(x => Console.WriteLine($"{x.Percentage}% processed."));
                var pipeline = pipelineFactory.Create(Settings.PeopleTargetFilePath, progress, cancellation.Token);
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
                var exception = executionResult.Exception.FlattenIfAggregate();

                Console.WriteLine("Faulted. Exception:");
                Console.WriteLine(exception);
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
