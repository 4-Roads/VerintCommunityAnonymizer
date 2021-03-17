using System;
using System.Text;
using RestSharp;
using RestSharp.Authenticators;

namespace FourRoads.TelligentCommunity.Utilities.VerintCommunityAnonymizer
{
    internal class TelligentApiKeyAuthenticator : IAuthenticator
    {
        private readonly bool   _impersonate;
        private readonly string _impersonateUsername;
        private readonly string _keyBase64;

        public TelligentApiKeyAuthenticator(string username, string apiKey, string impersonateUsername)
            : this(username, apiKey)
        {
            _impersonate = true;
            _impersonateUsername = impersonateUsername;
        }

        public TelligentApiKeyAuthenticator(string username, string apiKey)
        {
            _impersonate = false;
            _keyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{username}"));
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddHeader("Rest-User-Token", _keyBase64);
            if (_impersonate) request.AddHeader("Rest-Impersonate-User", _impersonateUsername);
        }
    }
}