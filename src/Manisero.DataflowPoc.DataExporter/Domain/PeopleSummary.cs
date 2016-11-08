using System;

namespace Manisero.DataflowPoc.DataExporter.Domain
{
    public class PeopleSummary
    {
        public int Count { get; set; }

        public DateTime? DateOfBirthMin { get; set; }

        public DateTime? DateOfBirthMax { get; set; }
    }
}
