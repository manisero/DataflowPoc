using System.IO;
using Manisero.DataflowPoc.Models;

namespace Manisero.DataflowPoc.Logic
{
    public class DataWriter
    {
        public void Write(StreamWriter writer, Data data)
        {
            writer.WriteLine(data.ToString());
        }
    }
}
