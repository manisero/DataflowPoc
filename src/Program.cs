﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Manisero.DataflowPoc.Logic;
using Manisero.DataflowPoc.Pipelines;
using Manisero.DataflowPoc.Pipelines.GenericBlockFactories;
using Manisero.DataflowPoc.Pipelines.PeopleBatchesStream;
using Manisero.DataflowPoc.Pipelines.PeopleStream;

namespace Manisero.DataflowPoc
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Press any key to start...");
            //Console.ReadKey();
            //Console.WriteLine();
            //Console.WriteLine();

            RunPeopleBatchesPipeline();
            //RunPeoplePipeline();
            //RunSynchronous();
        }

        private static void RunPeopleBatchesPipeline()
        {
            var peopleBatchesPipelineFactory = new PeopleBatchesPipelineFactory(new Pipelines.PeopleBatchesStream.BlockFactories.ReadingBlockFactory(new FileLinesCounter(),
                                                                                                                                                     new DataReader(new DataParser()),
                                                                                                                                                     new StreamLinesReader(),
                                                                                                                                                     new DataParser()),
                                                                                new PersonValidator(),
                                                                                new PersonFieldsComputer(),
                                                                                new Pipelines.PeopleBatchesStream.BlockFactories.WritingBlockFactory(new DataWriter()),
                                                                                new ProgressReportingBlockFactory(),
                                                                                new StraightPipelineFactory());

            var pipelineExecutor = new PipelineExecutor();

            using (var cancellationSource = new CancellationTokenSource())
            {
                var progress = new Progress<PipelineProgress>(x => Console.WriteLine($"{x.Percentage}% processed."));

                var pipeline = peopleBatchesPipelineFactory.Create(Settings.PeopleJsonFilePath,
                                                                   Settings.PeopleTargetFilePath,
                                                                   progress,
                                                                   cancellationSource);

                Task.Run(() => WaitForCancellation(cancellationSource));

                var executionResult = pipelineExecutor.Execute(pipeline).Result;
                HandleExecutionResult(executionResult);
            }

            PrintDataPoolSize();
        }

        private static void RunPeoplePipeline()
        {
            var peoplePipelineFactory = new PeoplePipelineFactory(new Pipelines.PeopleStream.BlockFactories.ReadingBlockFactory(new FileLinesCounter(),
                                                                                                                                new DataReader(new DataParser()),
                                                                                                                                new StreamLinesReader(),
                                                                                                                                new DataParser()),
                                                                  new PersonValidator(),
                                                                  new PersonFieldsComputer(),
                                                                  new Pipelines.PeopleStream.BlockFactories.WritingBlockFactory(new DataWriter()),
                                                                  new ThrowingBlockFactory(),
                                                                  new EmptyBlockFactory(),
                                                                  new ProgressReportingBlockFactory(),
                                                                  new RailroadPipelineFactory());
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

        private static void RunSynchronous()
        {
            Console.WriteLine("Running synchronous...");

            var synchronousPeopleProcessor = new SynchronousPeopleProcessor(new FileLinesCounter(),
                                                                            new StreamLinesReader(),
                                                                            new DataParser(),
                                                                            new DataReader(new DataParser()),
                                                                            new PersonValidator(),
                                                                            new PersonFieldsComputer(),
                                                                            new DataWriter());

            var synchronousDuration = synchronousPeopleProcessor.Process(Settings.PeopleJsonFilePath,
                                                                         Settings.PeopleTargetFilePath,
                                                                         Settings.ErrorsFilePath);

            Console.WriteLine($"Synchronous took {synchronousDuration.TotalMilliseconds}ms.");

            PrintDataPoolSize();
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

        private static void PrintDataPoolSize() => Console.WriteLine($"{nameof(DataPool)} size: {DataPool.Current.GetPooledCound()}");
    }
}
