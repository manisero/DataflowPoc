using System.Threading;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Pipeline
{
    public interface IPipelineFactory
    {
        StartableBlock<DataBatch<Person>> Create(CancellationTokenSource cancellation);
    }

    public class PipelineFactory : IPipelineFactory
    {
        private readonly IReadBlockFactory _readBlockFactory;

        public PipelineFactory(IReadBlockFactory readBlockFactory)
        {
            _readBlockFactory = readBlockFactory;
        }

        public StartableBlock<DataBatch<Person>> Create(CancellationTokenSource cancellation)
        {
            // TODO: Writing summary before people in the csv file

            return _readBlockFactory.Create(cancellation.Token);
        }
    }
}
