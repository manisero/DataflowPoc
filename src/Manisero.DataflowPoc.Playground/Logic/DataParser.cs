using System;
using Manisero.DataflowPoc.Playground.Models;
using Newtonsoft.Json;

namespace Manisero.DataflowPoc.Playground.Logic
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
