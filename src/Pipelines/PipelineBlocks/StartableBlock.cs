﻿using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Pipelines.PipelineBlocks
{
    public class StartableBlock<TOutput>
    {
        public Action Start { get; set; }

        public ISourceBlock<TOutput> Output { get; set; }

        public int EstimatedOutputCount { get; set; }

        public Task Completion { get; set; }
    }
}