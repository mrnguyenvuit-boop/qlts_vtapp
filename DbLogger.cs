using System;
using System.Threading.Tasks;
using Npgsql;

namespace ClientPrinterTray
{
    public class DbLogger
    {
        private readonly string _conn;

        public DbLogger(AppSettings settings)
        {
            _conn = settings.DbConnection; // 🔥 nhớ thêm vào AppSettings.json
        }

        public async Task InsertAsync(string jobId, string message, bool success)
        {
            await using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO print_logs(job_id, message, success, created)
                VALUES(@job, @msg, @ok, NOW())", con);

            cmd.Parameters.AddWithValue("@job", jobId);
            cmd.Parameters.AddWithValue("@msg", message);
            cmd.Parameters.AddWithValue("@ok", success);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
