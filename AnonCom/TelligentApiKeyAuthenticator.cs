using System;
using System.Text;
using RestSharp;
using RestSharp.Authenticators;

namespace FourRoads.TelligentCommunity.Utilities.VerintCommunityAnonymizer
{
    internal class TelligentApiKeyAuthenticator : IAuthenticator
    {
        private readonly string _keyBase64;

        public TelligentApiKeyAuthenticator(string username, string apiKey)
        {
            var key = $"{apiKey}:{username}";
            _keyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddHeader("Rest-User-Token", _keyBase64);
        }
    }
}