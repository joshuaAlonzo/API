using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Api.Main
{
    public static class ConnEnvFile
    {
        public static IReadOnlyDictionary<string, string> LoadValues()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string envFilePath = Path.Combine(AppContext.BaseDirectory, "conn.env");
                if (!File.Exists(envFilePath))
                {
                    envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "conn.env");
                }

                if (File.Exists(envFilePath))
                {
                    foreach (var line in File.ReadAllLines(envFilePath))
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#') || trimmed.StartsWith(';'))
                            continue;

                        var parts = trimmed.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            dict[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }
            }
            catch
            {
                // Ignore environment file read errors
            }

            return dict;
        }

        public static IEnumerable<KeyValuePair<string, string?>> LoadConfigurationValues() =>
            LoadValues().Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value));
    }
}
