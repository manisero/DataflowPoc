using CsvHelper;
using Manisero.DataflowPoc.DataExporter.Domain;
using Manisero.DataflowPoc.DataExporter.Pipeline.Models;

namespace Manisero.DataflowPoc.DataExporter.Logic
{
    public interface IPeopleBatchWriter
    {
        void Write(DataBatch<Person> batch, ICsvWriter csvWriter);
    }

    public class PeopleBatchWriter : IPeopleBatchWriter
    {
        public void Write(DataBatch<Person> batch, ICsvWriter csvWriter)
        {
            csvWriter.WriteRecords(batch.Data);
        }
    }
}
