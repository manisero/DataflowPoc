﻿using Manisero.DataflowPoc.Playground.Models;

namespace Manisero.DataflowPoc.Playground.Logic
{
    public class PersonValidator
    {
        public void Validate(Data data)
        {
            if (data.Person.Age > 30)
            {
                data.Error = $"Invalid age ({data.Person.Age}). People don't live longer than 30 years.";
            }

            if (Settings.SimulateTimeConsumingComputations)
            {
                ComputationsHelper.PerformTimeConsumingOperation();
            }
        }
    }
}
