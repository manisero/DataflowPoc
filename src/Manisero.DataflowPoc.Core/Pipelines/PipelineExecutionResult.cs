using System;

namespace Manisero.DataflowPoc.Core.Pipelines
{
    public class PipelineExecutionResult
    {
        public DateTime StartTs { get; set; }

        public DateTime FinishTs { get; set; }

        public bool Faulted => Exception != null;

        public Exception Exception { get; set; }

        public bool Canceled { get; set; }
    }
}
