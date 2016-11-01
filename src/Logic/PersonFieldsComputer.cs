using System;
using Dataflow.Models;

namespace Dataflow.Logic
{
    public class PersonFieldsComputer
    {
        public void Compute(Data data)
        {
            // TODO: Consider resolving processing date (DateTime.UtcNow) once and accepting it here
            data.Person.BirthYear = DateTime.UtcNow.Year - data.Person.Age;
        }
    }
}
