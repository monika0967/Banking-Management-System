using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BankingProject.Data
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("BankingDB")
                ?? throw new InvalidOperationException(
                    "BankingDB connection string is missing.");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
