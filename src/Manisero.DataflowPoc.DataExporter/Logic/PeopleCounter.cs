using Dapper;

namespace Manisero.DataflowPoc.DataExporter.Logic
{
    public interface IPeopleCounter
    {
        int Count();
    }

    public class PeopleCounter : IPeopleCounter
    {
        private readonly ISqlConnectionResolver _sqlConnectionResolver;

        public PeopleCounter(ISqlConnectionResolver sqlConnectionResolver)
        {
            _sqlConnectionResolver = sqlConnectionResolver;
        }

        public int Count()
        {
            const string sql = @"select count(*) from Person";

            return _sqlConnectionResolver.Resolve().ExecuteScalar<int>(sql);
        }
    }
}
