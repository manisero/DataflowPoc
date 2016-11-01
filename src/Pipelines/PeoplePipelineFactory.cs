using System;
using System.Threading;
using Dataflow.Logic;
using Dataflow.Models;
using Dataflow.Pipelines.BlockFactories;

namespace Dataflow.Pipelines
{
    public class PeoplePipelineFactory
    {
        private readonly ReadingBlockFactory _readingBlockFactory;
        private readonly PersonValidator _personValidator;
        private readonly PersonFieldsComputer _personFieldsComputer;
        private readonly WritingBlockFactory _writingBlockFactory;
        private readonly ThrowingBlockFactory _throwingBlockFactory;
        private readonly EmptyBlockFactory _emptyBlockFactory;
        private readonly HandleErrorBlockFactory _handleErrorBlockFactory;
        private readonly ProgressReportingBlockFactory _progressReportingBlockFactory;
        private readonly PipelineFactory _pipelineFactory;

        public PeoplePipelineFactory(ReadingBlockFactory readingBlockFactory,
                                     PersonValidator personValidator,
                                     PersonFieldsComputer personFieldsComputer,
                                     WritingBlockFactory writingBlockFactory,
                                     ThrowingBlockFactory throwingBlockFactory,
                                     EmptyBlockFactory emptyBlockFactory,
                                     HandleErrorBlockFactory handleErrorBlockFactory,
                                     ProgressReportingBlockFactory progressReportingBlockFactory,
                                     PipelineFactory pipelineFactory)
        {
            _readingBlockFactory = readingBlockFactory;
            _personValidator = personValidator;
            _personFieldsComputer = personFieldsComputer;
            _writingBlockFactory = writingBlockFactory;
            _throwingBlockFactory = throwingBlockFactory;
            _emptyBlockFactory = emptyBlockFactory;
            _handleErrorBlockFactory = handleErrorBlockFactory;
            _progressReportingBlockFactory = progressReportingBlockFactory;
            _pipelineFactory = pipelineFactory;
        }

        public StartableBlock<Data> Create(string peopleJsonFilePath, string targetFilePath, IProgress<PipelineProgress> progress, CancellationTokenSource cancellationSource)
        {
            // Create blocks

            // TODO: Unified Dataflow blocks creation (BoundedCapacity etc.)
            // TODO: Performance measurement

            // TODO: Progress reporting approach 1: before anything
            var readBlock = _readingBlockFactory.Create(peopleJsonFilePath, cancellationSource.Token);
            var validateBlock = ProcessingBlock<Data>.Create(x =>
                                                                 {
                                                                     if (x.IsValid)
                                                                     {
                                                                         _personValidator.Validate(x);
                                                                     }
                                                                 },
                                                             readBlock.EstimatedOutputCount,
                                                             cancellationSource.Token);
            var computeFieldsBlock = ProcessingBlock<Data>.Create(x =>
                                                                      {
                                                                          if (x.IsValid)
                                                                          {
                                                                              _personFieldsComputer.Compute(x);
                                                                          }
                                                                      },
                                                                  validateBlock.EstimatedOutputCount,
                                                                  cancellationSource.Token);
            var writeBlock = _writingBlockFactory.Create(targetFilePath, computeFieldsBlock.EstimatedOutputCount, cancellationSource.Token);
            var throwBlock = Settings.ThrowTest
                                 ? _throwingBlockFactory.Create<Data>(cancellationSource.Token)
                                 : _emptyBlockFactory.Create<Data>(writeBlock.EstimatedOutputCount, cancellationSource.Token);
            var handleErrorBlock = _handleErrorBlockFactory.Create(cancellationSource.Token);
            var progressBlock = _progressReportingBlockFactory.Create<Data>(progress, throwBlock.EstimatedOutputCount, cancellationSource.Token);

            return _pipelineFactory.Create(cancellationSource,
                                           readBlock,
                                           new[] { validateBlock, computeFieldsBlock, writeBlock, throwBlock },
                                           handleErrorBlock,
                                           progressBlock,
                                           x => x.IsValid);
        }
    }
}
