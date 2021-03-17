using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FourRoads.TelligentCommunity.Utilities.VerintCommunityAnonymizer.Types;
using NLog;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;

namespace FourRoads.TelligentCommunity.Utilities.VerintCommunityAnonymizer
{
    public class CommunityAnonymizer
    {
        private readonly Config _config;

        private readonly ILogger _logger;

        private CommunityAnonymizer(ILogger logger, Config config)
        {
            _logger = logger;
            _config = config;
        }

        public event ProgressDataHandler OnProgress;

        public static CommunityAnonymizer CreateInstance(ILogger logger, string configFileName)
        {
            return new(logger, Configure(configFileName));
        }

        public void AnonymizeIt()
        {
            var client = new RestClient(_config.SiteUrl);
            client.UseSystemTextJson();
            client.Authenticator = new TelligentApiKeyAuthenticator(_config.Username, _config.ApiKey);
            if (_config.IgnoreSslErrors)
                client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            var pageSize = _config.PageSize;
            var pageIndex = 0;
            var count = 0;
            var total = 0;

            do
            {
                var res = client.Get<PagedResult<User>>(GetUsersRequest(pageSize, pageIndex));
                if (res.IsSuccessful)
                {
                    total = res.Data.TotalCount;
                    ProcessBatch(client, res.Data.Users, count, total);
                    count += res.Data.Users.Count;
                    pageIndex++;
                }
            } while (count < total);
        }

        private void ProcessBatch(RestClient restClient, IEnumerable<User> users, int current, int total)
        {
            foreach (var user in users)
            {
                current++;
                try
                {
                    if (_config.ExcludedUserIds.Contains(user.Id))
                    {
                        _logger.Info($"{user.Id}, {user.Username}=> EXCLUDED, {user.PrivateEmail}=> EXCLUDED");
                        continue;
                    }

                    var res = restClient.Post<UserResult>(GetUpdateUserRequest(user));
                    if (res.IsSuccessful)
                    {
                        var upd = res.Data.User;
                        _logger.Info(
                            $"{user.Id}, {user.Username}=>{upd.Username}, {user.PrivateEmail}=>{upd.PrivateEmail}");

                        //delete avatar
                        restClient.Post<Result>(GedDeleteUserAvatar(user.Id));
                        continue;
                    }

                    _logger.Error($"{user.Id} failed. Error:{res.Data}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Exception while processing {user.Id} ({user.Username}");
                }
                finally
                {
                    OnProgress?.Invoke(this, new OnProgressArgs {Current = current, Total = total, UserId = user.Id});
                }
            }
        }

        private IRestRequest GedDeleteUserAvatar(int userId)
        {
            return new RestRequest("api.ashx/v2/users/{id}/avatar.json")
                .AddUrlSegment("id", userId)
                .AddHeader("Rest-Method", "DELETE");
        }

        private IRestRequest GetUpdateUserRequest(User user)
        {
            return new RestRequest("api.ashx/v2/users/{id}.json")
                .AddUrlSegment("id", user.Id)
                .AddParameter("Username", $"user{user.Id}")
                .AddParameter("PrivateEmail", $"user{user.Id}@localhost.local")
                .AddParameter("PublicEmail", $"user{user.Id}.public@localhost.local")
                .AddParameter("DisplayName", $"User {user.Id}")
                .AddParameter("Location", $"Maycomb, {user.Id}")
                .AddParameter("Birthday", new DateTime(2000, 1, 1))
                .AddParameter("IsBirthdaySet", true)
                .AddHeader("Rest-Method", "PUT");
        }

        private static IRestRequest GetUsersRequest(int pageSize, int pageIndex)
        {
            return new RestRequest("api.ashx/v2/users.json")
                .AddParameter("SortBy", "JoinedDate")
                .AddParameter("SortOrder", "Ascending")
                .AddParameter("PageSize", pageSize)
                .AddParameter("PageIndex", pageIndex);
        }

        private static IRestRequest GetUserInfoRequest(RestClient client, int userId)
        {
            return new RestRequest("api.ashx/v2/user.json").AddParameter("Id", userId);
        }

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
            var config = new Config
            {
                Username = "admin",
                ApiKey = "apikey",
                SiteUrl = "http://locahost:50078/",
                ExcludedUserIds = new List<int>
                {
                    2100, 2102, 2103
                }
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
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
            public int PageSize { get; } = 100;
            public bool IgnoreSslErrors { get; set; }
        }
    }
}