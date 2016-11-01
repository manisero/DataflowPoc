﻿using System;
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
        private const bool THROW = false;

        private const string PEOPLE_JSON_FILE_PATH = @"\\VBOXSVR\temp\people.json";
        private const string PEOPLE_RESULT_FILE_PATH = @"\\VBOXSVR\temp\people_result.txt";

        static void Main(string[] args)
        {
            var readingBlockFactory = new ReadingBlockFactory(false, new FileLinesCounter(), new DataReader(), new StreamLinesReader(), new DataParser());
            var writingBlockFactory = new WritingBlockFactory(new DataWriter());
            var throwingBlockFactory = new ThrowingBlockFactory();
            var emptyBlockFactory = new EmptyBlockFactory();
            var pipelineFactory = new PipelineFactory();
            var pipelineExecutor = new PipelineExecutor();

            var cancellationSource = new CancellationTokenSource();

            // Create blocks
            // TODO: Progress reporting approach 1: before anything
            var readBlock = readingBlockFactory.Create(PEOPLE_JSON_FILE_PATH, cancellationSource.Token);
            var writeBlock = writingBlockFactory.Create(PEOPLE_RESULT_FILE_PATH, readBlock.EstimatedOutputCount, cancellationSource.Token);
            var throwBlock = THROW ? throwingBlockFactory.Create<Data>(cancellationSource.Token) : emptyBlockFactory.Create<Data>(writeBlock.EstimatedOutputCount, cancellationSource.Token);
            // TODO: Data-level error handling (reporting / logging)
            // TODO: Progress reporting approach 2: after everything

            // Execute pipeline
            var pipeline = pipelineFactory.Create(readBlock, new[] { writeBlock, throwBlock }, cancellationSource);
            var pipelineCompletion = pipelineExecutor.Execute(pipeline);

            //WaitForCancellation(pipelineCompletion, cancellationSource);

            var executionResult = pipelineCompletion.Result;
            HandleExecutionResult(executionResult);

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
