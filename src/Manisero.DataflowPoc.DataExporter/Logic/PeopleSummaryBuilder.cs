using Manisero.DataflowPoc.DataExporter.Domain;

namespace Manisero.DataflowPoc.DataExporter.Logic
{
    public interface IPeopleSummaryBuilder
    {
        void Include(Person person, PeopleSummary peopleSummary);
    }

    public class PeopleSummaryBuilder : IPeopleSummaryBuilder
    {
        public void Include(Person person, PeopleSummary peopleSummary)
        {
            peopleSummary.Count++;

            if (peopleSummary.DateOfBirthMin == null || person.DateOfBirth < peopleSummary.DateOfBirthMin)
            {
                peopleSummary.DateOfBirthMin = person.DateOfBirth;
            }

            if (peopleSummary.DateOfBirthMax == null || person.DateOfBirth > peopleSummary.DateOfBirthMax)
            {
                peopleSummary.DateOfBirthMax = person.DateOfBirth;
            }
        }
    }
}
