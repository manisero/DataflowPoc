using System.Collections.Generic;
using System.IO;
using Dataflow.Models;
using Newtonsoft.Json;

namespace Dataflow.Logic
{
    public class DataReader
    {
        public IEnumerable<Data> Read(StreamReader peopleJsonStream, int peopleToRead)
        {
            for (var i = 0; i < peopleToRead && !peopleJsonStream.EndOfStream; i++)
            {
                var line = peopleJsonStream.ReadLine();

                if (!string.IsNullOrEmpty(line))
                {
                    var person = JsonConvert.DeserializeObject<Person>(line);

                    yield return new Data
                        {
                            Person = person
                        };
                }
                else
                {
                    yield return new Data
                        {
                            Error = "Empty row."
                        };
                }
            }
        }
    }
}
