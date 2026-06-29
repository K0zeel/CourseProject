using System.IO;
using System.Text.Json;

namespace DPWrestlingScoreboard.Data
{
    /// <summary>
    /// Настройки из appsettings.json (строка подключения не в коде).
    /// </summary>
    public static class AppConfiguration
    {
        private const string DefaultConnection =
            "Server=localhost;Database=kokos;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;";

        public static string ConnectionString { get; }

        static AppConfiguration()
        {
            ConnectionString = LoadConnectionString();
        }

        private static string LoadConnectionString()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(path))
                    return DefaultConnection;

                var json = File.ReadAllText(path);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                    cs.TryGetProperty("WrestlingDb", out var value))
                {
                    var connection = value.GetString();
                    if (!string.IsNullOrWhiteSpace(connection))
                        return connection;
                }
            }
            catch
            {
                // fallback
            }

            return DefaultConnection;
        }
    }
}
