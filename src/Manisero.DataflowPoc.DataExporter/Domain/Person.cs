using System;

namespace Manisero.DataflowPoc.DataExporter.Domain
{
    public class Person
    {
        public int PersonId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public double ArmToLegLengthRatio { get; set; }

        public string PhoneNumber { get; set; }

        public decimal Salary { get; set; }
    }
}
