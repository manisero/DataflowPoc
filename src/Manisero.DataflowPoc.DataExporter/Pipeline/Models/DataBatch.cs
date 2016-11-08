using System.Collections.Generic;

namespace Manisero.DataflowPoc.DataExporter.Pipeline.Models
{
    public class DataBatch<TData>
    {
        public int Number { get; set; }

        public int DataOffset { get; set; }

        public int IntendedSize { get; set; }

        public ICollection<TData> Data { get; set; }
    }
}
