using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace CollegeComputerTechFinal.DAL
{
    public static class DatabaseHelper
    {
        public static string ConnectionString { get; private set; }

        static DatabaseHelper()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["CollegeDB"]?.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
                throw new Exception("Строка подключения не найдена в App.config");
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка подключения: {ex.Message}");
                return false;
            }
        }

        public static void AddNotification(string title, string message, string forRole)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = @"INSERT INTO Уведомления (заголовок, сообщение, для_кого) 
                         VALUES (@title, @message, @forRole)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@message", message);
                    cmd.Parameters.AddWithValue("@forRole", forRole);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static async Task<string> GetPublicIPAddressAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {                    var response = await client.GetStringAsync("https://api.ipify.org");
                    return response.Trim();
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }
    }
}