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
                    JsonConvert.PopulateObject(data.PersonJson, data.Person);
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
