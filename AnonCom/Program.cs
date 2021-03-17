using System;
using System.Reflection;
using NLog;
using NUnit.Framework;

namespace AnonCom
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var configFileName = args.Length > 0 ? args[0] : "config.json";
            var tool = CommunityAnonymizer.CreateInstance(logger, configFileName);
            Console.WriteLine("Hello World!");
        }
    }
}