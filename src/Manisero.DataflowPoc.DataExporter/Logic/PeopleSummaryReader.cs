using Dapper;
using Manisero.DataflowPoc.DataExporter.Domain;

namespace Manisero.DataflowPoc.DataExporter.Logic
{
    public interface IPeopleSummaryReader
    {
        PeopleSummary Read();
    }

    public class PeopleSummaryReader : IPeopleSummaryReader
    {
        private readonly ISqlConnectionResolver _sqlConnectionResolver;

        public PeopleSummaryReader(ISqlConnectionResolver sqlConnectionResolver)
        {
            _sqlConnectionResolver = sqlConnectionResolver;
        }

        public PeopleSummary Read()
        {
            const string sql = @"select * from PeopleSummary";

            return _sqlConnectionResolver.Resolve().QuerySingle<PeopleSummary>(sql);
        }
    }
}
