namespace Dataflow.Models
{
    public class Data
    {
        public Person Person { get; set; }

        public string Error { get; set; }

        public override string ToString()
        {
            return Error ?? Person.ToString();
        }
    }
}
