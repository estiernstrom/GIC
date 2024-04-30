using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIC.Utilities
{
    public class TimeStampHelper
    {

        private DatabaseService _databaseService;

        public TimeStampHelper()
        {
            _databaseService = new DatabaseService();
        }



        public async Task<DateTime?> GetLastUpdateTimestampAsync()
        {
            using (var connection = new NpgsqlConnection(_databaseService.ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("SELECT MAX(last_updated) FROM version_info", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return result != DBNull.Value ? (DateTime?)result : null;
                }
            }
        }


        public void SaveLastKnownUpdateTimestamp(DateTime timestamp)
        {
            var timestampString = timestamp.ToString("o");
            Preferences.Set("LastUpdateTimestamp", timestampString);
            System.Diagnostics.Debug.WriteLine($"Saved timestamp: {timestampString}");
        }

        public DateTime? LoadLastKnownUpdateTimestamp()
        {
            var timestampString = Preferences.Get("LastUpdateTimestamp", defaultValue: null);
            System.Diagnostics.Debug.WriteLine($"Retrieved timestamp string: {timestampString}");

            if (timestampString != null)
            {
                try
                {
                    var timestamp = DateTime.Parse(timestampString, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    System.Diagnostics.Debug.WriteLine($"Parsed timestamp: {timestamp:o}");
                    return timestamp;
                }
                catch (FormatException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parsing timestamp: {ex.Message}");
                    return null;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No timestamp found in preferences.");
                return null;
            }
        }
    }

}
