using System.Diagnostics.Tracing;

namespace Dataflow.Etw
{
    [EventSource(Name = "Manisero.DataflowPoc")]
    public class Events : EventSource
    {
        public static readonly Events Write = new Events();

        public const int BlockEnterId = 1;
        
        [Event(BlockEnterId, Level = EventLevel.Informational)]
        public void BlockEnter(string blockName, int dataId)
        {
            if (IsEnabled())
            {
                WriteEvent(BlockEnterId, blockName, dataId);
            }
        }

        public const int BlockExitId = 2;

        [Event(BlockExitId, Level = EventLevel.Informational)]
        public void BlockExit(string blockName, int dataId, long elapsedMs)
        {
            if (IsEnabled())
            {
                WriteEvent(BlockExitId, blockName, dataId, elapsedMs);
            }
        }

        public const int DataCreationId = 3;

        [Event(DataCreationId, Level = EventLevel.Informational)]
        public void DataCreation(int dataId)
        {
            if (IsEnabled())
            {
                WriteEvent(DataCreationId, dataId);
            }
        }
    }
}
