namespace Manisero.DataflowPoc.Models
{
    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public int BirthYear { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName}, {Age} y.o. (born {BirthYear})";
        }
    }
}
