using System;

namespace Manisero.DataflowPoc.Pipelines
{
    public class PipelineExecutionResult
    {
        public DateTime StartTs { get; set; }

        public DateTime FinishTs { get; set; }

        public bool Faulted { get; set; }

        public Exception Exception { get; set; }

        public bool Canceled { get; set; }
    }
}
