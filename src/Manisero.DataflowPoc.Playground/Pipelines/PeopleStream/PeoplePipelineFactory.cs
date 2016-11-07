using System;
using System.Threading;
using Manisero.DataflowPoc.Core.Pipelines;
using Manisero.DataflowPoc.Core.Pipelines.GenericBlockFactories;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.Playground.Logic;
using Manisero.DataflowPoc.Playground.Models;
using Manisero.DataflowPoc.Playground.Pipelines.PeopleStream.BlockFactories;

namespace Manisero.DataflowPoc.Playground.Pipelines.PeopleStream
{
    public class PeoplePipelineFactory
    {
        private readonly ReadingBlockFactory _readingBlockFactory;
        private readonly PersonValidator _personValidator;
        private readonly PersonFieldsComputer _personFieldsComputer;
        private readonly WritingBlockFactory _writingBlockFactory;
        private readonly ThrowingBlockFactory _throwingBlockFactory;
        private readonly EmptyBlockFactory _emptyBlockFactory;
        private readonly ProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly RailroadPipelineFactory _railroadPipelineFactory;

        public PeoplePipelineFactory(ReadingBlockFactory readingBlockFactory,
                                     PersonValidator personValidator,
                                     PersonFieldsComputer personFieldsComputer,
                                     WritingBlockFactory writingBlockFactory,
                                     ThrowingBlockFactory throwingBlockFactory,
                                     EmptyBlockFactory emptyBlockFactory,
                                     ProgressReportingBlockFactory progressReportingBlockFactory,
                                     RailroadPipelineFactory railroadPipelineFactory)
        {
            _readingBlockFactory = readingBlockFactory;
            _personValidator = personValidator;
            _personFieldsComputer = personFieldsComputer;
            _writingBlockFactory = writingBlockFactory;
            _throwingBlockFactory = throwingBlockFactory;
            _emptyBlockFactory = emptyBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _railroadPipelineFactory = railroadPipelineFactory;
        }

        public StartableBlock<Data> Create(string peopleJsonFilePath,
                                           string targetFilePath,
                                           string errorsFilePath,
                                           IProgress<PipelineProgress> progress,
                                           CancellationTokenSource cancellationSource)
        {
            var dataPool = new DataPool();

            // Create blocks
            // TODO: Progress reporting approach 1: before anything
            var readBlock = _readingBlockFactory.Create(peopleJsonFilePath, dataPool, cancellationSource.Token);
            var validateBlock = ProcessingBlock<Data>.Create("Validate",
                                                             Data.IdGetter,
                                                             x => _personValidator.Validate(x),
                                                             cancellationSource.Token,
                                                             Settings.ProcessInParallel ? Settings.MaxDegreeOfParallelism : 1);
            var computeFieldsBlock = ProcessingBlock<Data>.Create("ComputeFields",
                                                                  Data.IdGetter,
                                                                  x => _personFieldsComputer.Compute(x),
                                                                  cancellationSource.Token,
                                                                  Settings.ProcessInParallel ? Settings.MaxDegreeOfParallelism : 1);
            var writeBlock = _writingBlockFactory.Create(targetFilePath, cancellationSource.Token);
            var throwBlock = Settings.ThrowTest
                                 ? _throwingBlockFactory.Create(Data.IdGetter, cancellationSource.Token)
                                 : _emptyBlockFactory.Create<Data>(cancellationSource.Token);
            var handleErrorBlock = _writingBlockFactory.Create(errorsFilePath, cancellationSource.Token);
            var progressBlock = _progressReportingBlockFactory.Create(Data.IdGetter, progress, readBlock.EstimatedOutputCount, Settings.ProgressBatchSize, cancellationSource.Token);

            return _railroadPipelineFactory.Create(readBlock,
                                                   new[] { validateBlock, computeFieldsBlock, writeBlock, throwBlock },
                                                   handleErrorBlock,
                                                   progressBlock,
                                                   x => x.IsValid,
                                                   cancellationSource);
        }
    }
}
