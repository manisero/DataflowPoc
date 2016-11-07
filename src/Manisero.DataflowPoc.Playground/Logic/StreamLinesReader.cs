using System.Collections.Generic;
using System.IO;

namespace Manisero.DataflowPoc.Playground.Logic
{
    public class StreamLinesReader
    {
        public IEnumerable<string> Read(StreamReader streamReader, int linesToRead)
        {
            for (var i = 0; i < linesToRead && !streamReader.EndOfStream; i++)
            {
                yield return streamReader.ReadLine();
            }
        }
    }
}
