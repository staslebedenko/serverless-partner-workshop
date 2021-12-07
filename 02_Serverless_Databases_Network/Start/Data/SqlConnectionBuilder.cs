using Microsoft.Data.SqlClient;

namespace Data
{
    public class SqlConnectionBuilder
    {
        public static string GetConnectionString(string connectionString, string passwordKey)
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { Password = passwordKey };

            return builder.ConnectionString;
        }
    }
}
