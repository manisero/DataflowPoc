using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dataflow.Logic;
using Dataflow.Models;
using Dataflow.Pipelines.GenericBlockFactories;
using Dataflow.Pipelines.PeopleBatchesStream.BlockFactories;
using Dataflow.Pipelines.PipelineBlocks;

namespace Dataflow.Pipelines.PeopleBatchesStream
{
    public class PeopleBatchesPipelineFactory
    {
        private readonly ReadingBlockFactory _readingBlockFactory;
        private readonly PersonValidator _personValidator;
        private readonly PersonFieldsComputer _personFieldsComputer;
        private readonly WritingBlockFactory _writingBlockFactory;
        private readonly ThrowingBlockFactory _throwingBlockFactory;
        private readonly EmptyBlockFactory _emptyBlockFactory;
        private readonly ProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly PipelineFactory _pipelineFactory;

        public PeopleBatchesPipelineFactory(ReadingBlockFactory readingBlockFactory,
                                     PersonValidator personValidator,
                                     PersonFieldsComputer personFieldsComputer,
                                     WritingBlockFactory writingBlockFactory,
                                     ThrowingBlockFactory throwingBlockFactory,
                                     EmptyBlockFactory emptyBlockFactory,
                                     ProgressReportingBlockFactory progressReportingBlockFactory,
                                     PipelineFactory pipelineFactory)
        {
            _readingBlockFactory = readingBlockFactory;
            _personValidator = personValidator;
            _personFieldsComputer = personFieldsComputer;
            _writingBlockFactory = writingBlockFactory;
            _throwingBlockFactory = throwingBlockFactory;
            _emptyBlockFactory = emptyBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _pipelineFactory = pipelineFactory;
        }

        public StartableBlock<IList<Data>> Create(string peopleJsonFilePath,
                                                  string targetFilePath,
                                                  string errorsFilePath,
                                                  IProgress<PipelineProgress> progress,
                                                  CancellationTokenSource cancellationSource)
        {
            // TODO: Railroad-driven error handling

            // Create blocks
            var readBlock = _readingBlockFactory.Create(peopleJsonFilePath, cancellationSource.Token);
            var validateBlock = ProcessingBlock<IList<Data>>.Create("Validate",
                                                                    x =>
                                                                        {
                                                                            foreach (var item in x.Where(item => item.IsValid))
                                                                            {
                                                                                _personValidator.Validate(item);
                                                                            }
                                                                        },
                                                                    cancellationSource.Token,
                                                                    Settings.SimulateTimeConsumingComputations ? 3 : 1);
            var computeFieldsBlock = ProcessingBlock<IList<Data>>.Create("ComputeFields",
                                                                         x =>
                                                                             {
                                                                                 foreach (var item in x.Where(item => item.IsValid))
                                                                                 {
                                                                                     _personFieldsComputer.Compute(item);
                                                                                 }
                                                                             },
                                                                         cancellationSource.Token,
                                                                         Settings.SimulateTimeConsumingComputations ? 3 : 1);
            var writeBlock = _writingBlockFactory.Create(targetFilePath, cancellationSource.Token);
            var throwBlock = Settings.ThrowTest
                                 ? _throwingBlockFactory.Create<IList<Data>>(cancellationSource.Token)
                                 : _emptyBlockFactory.Create<IList<Data>>(cancellationSource.Token);
            var handleErrorBlock = _writingBlockFactory.Create(errorsFilePath, cancellationSource.Token);
            var progressBlock = _progressReportingBlockFactory.Create<IList<Data>>(progress, readBlock.EstimatedOutputCount, 1, cancellationSource.Token);

            return _pipelineFactory.Create(readBlock,
                                           new[] { validateBlock, computeFieldsBlock, writeBlock, throwBlock },
                                           handleErrorBlock,
                                           progressBlock,
                                           x => true,
                                           cancellationSource);
        }
    }
}
