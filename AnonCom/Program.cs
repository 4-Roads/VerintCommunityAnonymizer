using System;
using System.IO;
using NLog;

namespace FourRoads.TelligentCommunity.Utilities.VerintCommunityAnonymizer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var configFileName = args.Length > 0 ? args[0] : "config.json";
            if (!File.Exists(configFileName))
            {
                Console.WriteLine(
                    @"Could not find file named ""config.json"" and no config filename was passed as an argument");
                return;
            }

            var anonymizer = CommunityAnonymizer.CreateInstance(logger, configFileName);
            anonymizer.OnProgress += (sender, args) =>
            {
                Console.WriteLine($"Processing user {args.UserId}, {args.Current} out of {args.Total}");
            };
            anonymizer.AnonymizeIt();
            Console.WriteLine(@"Finished! Review the log file in the \logs folder");
        }
    }
}