using Dataflow.Models;
using Newtonsoft.Json;

namespace Dataflow.Logic
{
    public class DataParser
    {
        public Data Parse(string personJson)
        {
            if (string.IsNullOrEmpty(personJson))
            {
                return new Data
                {
                    Error = "Empty row."
                };
            }
            
            // TODO: Handle parsing exception
            var person = JsonConvert.DeserializeObject<Person>(personJson);

            return new Data
                {
                    Person = person
                };
        }
    }
}
