using System;
using System.Data.SqlClient;

namespace Manisero.DataflowPoc.DataExporter.Logic
{
    public interface ISqlConnectionResolver
    {
        SqlConnection Resolve();
    }

    public class SqlConnectionResolver : ISqlConnectionResolver
    {
        private readonly string _connectionString;
        private readonly Lazy<SqlConnection> _connection;

        public SqlConnectionResolver(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new Lazy<SqlConnection>(CreateConnection);
        }

        public SqlConnection Resolve() => _connection.Value;

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
