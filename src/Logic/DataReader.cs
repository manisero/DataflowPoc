using System.Collections.Generic;
using System.IO;
using Dataflow.Models;

namespace Dataflow.Logic
{
    public class DataReader
    {
        private readonly DataParser _dataParser;

        public DataReader(DataParser dataParser)
        {
            _dataParser = dataParser;
        }

        public IEnumerable<Data> Read(StreamReader peopleJsonStream, int peopleToRead, DataPool dataPool)
        {
            for (var i = 0; i < peopleToRead && !peopleJsonStream.EndOfStream; i++)
            {
                var line = peopleJsonStream.ReadLine();

                var data = dataPool.Rent();
                data.PersonJson = line;
                _dataParser.Parse(data);

                yield return data;
            }
        }
    }
}
