using System.Diagnostics.Tracing;

namespace Dataflow.Etw
{
    [EventSource(Name = "Manisero.DataflowPoc")]
    public class Events : EventSource
    {
        public static readonly Events Write = new Events();

        public const int BlockEnterId = 1;
        public const int BlockExitId = 2;

        [Event(BlockEnterId, Level = EventLevel.Informational)]
        public void BlockEnter(string blockName, object dataId)
        {
            if (IsEnabled())
            {
                WriteEvent(BlockEnterId, blockName, dataId ?? string.Empty);
            }
        }

        [Event(BlockExitId, Level = EventLevel.Informational)]
        public void BlockExit(string blockName, object dataId, long elapsedMs)
        {
            if (IsEnabled())
            {
                WriteEvent(BlockExitId, blockName, dataId ?? string.Empty, elapsedMs);
            }
        }
    }
}
