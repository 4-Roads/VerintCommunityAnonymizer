using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NLog;

namespace AnonCom
{
    public partial class CommunityAnonymizer
    {
        public static CommunityAnonymizer CreateInstance(ILogger logger, string configFileName)
        {
            var config = Configure(configFileName);
            return new CommunityAnonymizer(logger,  config);
        }

        private readonly ILogger _logger;
        private readonly Config _config;

        private CommunityAnonymizer(ILogger logger, Config config)
        {
            _logger = logger;
            _config = config;
        }

    }
}