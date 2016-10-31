using System.IO;
using Dataflow.Models;

namespace Dataflow.Logic
{
    public class DataWriter
    {
        public void Write(StreamWriter writer, Data data)
        {
            writer.WriteLine(data.ToString());
        }
    }
}
