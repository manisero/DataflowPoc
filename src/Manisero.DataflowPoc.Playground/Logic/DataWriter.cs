using System.IO;
using Manisero.DataflowPoc.Playground.Models;

namespace Manisero.DataflowPoc.Playground.Logic
{
    public class DataWriter
    {
        public void Write(StreamWriter writer, Data data)
        {
            writer.WriteLine(data.ToString());
        }
    }
}
