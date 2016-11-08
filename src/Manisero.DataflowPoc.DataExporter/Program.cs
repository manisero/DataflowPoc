using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using Manisero.DataflowPoc.DataExporter.Domain;

namespace Manisero.DataflowPoc.DataExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DataExporter"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                var people = connection.Query<Person>(@"select * from Person")
                                        .AsList();

            }

            // TODO:
            // - read data (Dapper)
            // - write to csv (CsvHelper)
        }
    }
}
