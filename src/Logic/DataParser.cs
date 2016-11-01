using System;
using Dataflow.Models;
using Newtonsoft.Json;

namespace Dataflow.Logic
{
    public class DataParser
    {
        public Data Parse(string personJson)
        {
            var data = new Data();

            if (!string.IsNullOrEmpty(personJson))
            {
                try
                {
                    data.Person = JsonConvert.DeserializeObject<Person>(personJson);
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

            return data;
        }
    }
}
