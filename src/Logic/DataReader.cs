using System;
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
                var data = new Data();

                if (!string.IsNullOrEmpty(line))
                {
                    try
                    {
                        data.Person = JsonConvert.DeserializeObject<Person>(line);
                    }
                    catch (Exception e)
                    {
                        data.Error = e.Message;
                    }
                }
                else
                {
                    data.Error = "Empty row.";
                }

                yield return data;
            }
        }
    }
}
