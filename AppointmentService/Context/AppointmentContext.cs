using System.Data;
using System.Data.SqlClient;

namespace AppointmentService.Context
{
    public class AppointmentContext
    {
        private readonly IConfiguration _configuration;

        private readonly string _connectionString;

        public AppointmentContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SqlConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    }
}
