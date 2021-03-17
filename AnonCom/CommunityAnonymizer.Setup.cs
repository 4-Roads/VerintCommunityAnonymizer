using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AnonCom
{
    public partial class CommunityAnonymizer
    {
        private static Config Configure(string configFileName)
        {
            var alwaysExcludedUserIds = new List<int>
            {
                2100, 2101, 2102, 2103
            };
            var file = File.ReadAllText(configFileName);
            var config = JsonSerializer.Deserialize<Config>(file);
            if (config != null)
            {
                config.ExcludedUserIds = config.ExcludedUserIds.Concat(alwaysExcludedUserIds);
                return config;
            }

            throw new InvalidOperationException($"Could not process config file {configFileName}");
        }

        private void WriteDefaultConfig(string configFileName)
        {
            var config = new Config()
            {
                Username = "admin",
                ApiKey = "apikey",
                SiteUrl = "http://locahost:50078/",
                ExcludedUserIds = new List<int>
                {
                    2100, 2102, 2103, 2104, 2105, 2106
                }
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var jsonString = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configFileName, jsonString);
        }

        private class Config
        {
            public string Username { get; set; }
            public string ApiKey { get; set; }
            public string SiteUrl { get; set; }
            public IEnumerable<int> ExcludedUserIds { get; set; }
        }
    }
}