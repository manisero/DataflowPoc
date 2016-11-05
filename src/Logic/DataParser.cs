using System;
using Dataflow.Models;
using Newtonsoft.Json;

namespace Dataflow.Logic
{
    public class DataParser
    {
        public void Parse(Data data)
        {
            if (!string.IsNullOrEmpty(data.PersonJson))
            {
                try
                {
                    data.Person = JsonConvert.DeserializeObject<Person>(data.PersonJson);
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
        }
    }
}
