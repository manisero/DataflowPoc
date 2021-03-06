﻿using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.DataflowPoc.Core.Extensions;
using Manisero.DataflowPoc.Core.Pipelines.PipelineBlocks;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Logic;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Pipeline.BlockFactories
{
    public interface IReadSummaryBlockFactory
    {
        StartableBlock<DataBatch<PeopleSummary>> Create(CancellationToken cancellation);
    }

    public class ReadSummaryBlockFactory : IReadSummaryBlockFactory
    {
        private readonly IPeopleSummaryReader _peopleSummaryReader;

        public ReadSummaryBlockFactory(IPeopleSummaryReader peopleSummaryReader)
        {
            _peopleSummaryReader = peopleSummaryReader;
        }

        public StartableBlock<DataBatch<PeopleSummary>> Create(CancellationToken cancellation)
        {
            var readBlock = DataflowFacade.TransformBlock<DataBatch<PeopleSummary>>("ReadPeopleSummary",
                                                                                    DataBatch<PeopleSummary>.IdGetter,
                                                                                    x => x.Data = new[] { _peopleSummaryReader.Read() },
                                                                                    cancellation);

            return new StartableBlock<DataBatch<PeopleSummary>>(
                () =>
                    {
                        readBlock.Post(new DataBatch<PeopleSummary>
                            {
                                Number = -1,
                                DataOffset = 0,
                                IntendedSize = 1
                            });

                        readBlock.Complete();
                    },
                readBlock,
                1);
        }
    }
}
