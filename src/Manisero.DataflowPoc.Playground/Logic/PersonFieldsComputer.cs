using System;
using Manisero.DataflowPoc.Playground.Models;

namespace Manisero.DataflowPoc.Playground.Logic
{
    public class PersonFieldsComputer
    {
        public void Compute(Data data)
        {
            // TODO: Consider resolving processing date (DateTime.UtcNow) once and accepting it here

            if (data.Person.Age >= 0)
            {
                data.Person.BirthYear = DateTime.UtcNow.Year - data.Person.Age;
            }
            else
            {
                data.Error = $"Person has not yet been born (age: {data.Person.Age}). Cannot calculate their {nameof(data.Person.BirthYear)}.";
            }

            if (Settings.SimulateTimeConsumingComputations)
            {
                ComputationsHelper.PerformTimeConsumingOperation();
                ComputationsHelper.PerformTimeConsumingOperation();
                ComputationsHelper.PerformTimeConsumingOperation();
            }
        }
    }
}
