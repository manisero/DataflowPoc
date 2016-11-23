using System;
using System.Threading;
using System.Threading.Tasks;
using Manisero.DataflowPoc.Core.Pipelines;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Pipeline
{
    public interface IParallelPipelineFactory
    {
        StartableBlock<DataBatch<Person>> Create(string targetFilePath1, string targetFilePath2, IProgress<PipelineProgress> progress, CancellationToken cancellation);
    }

    public class ParallelPipelineFactory : IParallelPipelineFactory
    {
        private readonly IPipelineFactory _pipelineFactory;

        public ParallelPipelineFactory(IPipelineFactory pipelineFactory)
        {
            _pipelineFactory = pipelineFactory;
        }

        public StartableBlock<DataBatch<Person>> Create(string targetFilePath1, string targetFilePath2, IProgress<PipelineProgress> progress, CancellationToken cancellation)
        {
            var pipeline1 = _pipelineFactory.Create(targetFilePath1, progress, cancellation);
            var pipeline2 = _pipelineFactory.Create(targetFilePath2, progress, cancellation);

            return new StartableBlock<DataBatch<Person>>(() =>
                                                             {
                                                                 pipeline1.Start();
                                                                 pipeline2.Start();
                                                             },
                                                         pipeline2.Output,
                                                         pipeline2.EstimatedOutputCount,
                                                         Task.WhenAll(pipeline1.Completion, pipeline2.Completion),
                                                         true);
        }
    }
}
