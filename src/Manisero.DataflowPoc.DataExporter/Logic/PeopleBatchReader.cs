using System.Collections.Generic;
using Dapper;
using Manisero.DataflowPoc.DataExporter.Domain;

namespace Manisero.DataflowPoc.DataExporter.Logic
{
    public interface IPeopleBatchReader
    {
        IList<Person> Read(int offset, int batchSize);
    }

    public class PeopleBatchReader : IPeopleBatchReader
    {
        private readonly ISqlConnectionResolver _sqlConnectionResolver;

        public PeopleBatchReader(ISqlConnectionResolver sqlConnectionResolver)
        {
            _sqlConnectionResolver = sqlConnectionResolver;
        }

        public IList<Person> Read(int offset, int batchSize)
        {
            const string sql =
@"select *
from (select *, row_number() over (order by PersonId) as _rowNum
      from Person) as p
where _rowNum between @FirstRowNum and @LastRowNum";

            return _sqlConnectionResolver.Resolve()
                                         .Query<Person>(sql,
                                                        new
                                                            {
                                                                FirstRowNum = offset + 1,
                                                                LastRowNum = offset + batchSize
                                                            })
                                         .AsList();
        }
    }
}
