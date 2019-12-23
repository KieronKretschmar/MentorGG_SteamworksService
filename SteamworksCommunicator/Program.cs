using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharingCodeGatherer;
using System;

namespace SteamworksCommunicator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Start();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(o =>
                    {
                        o.AddConsole();
                        o.AddDebug();
                    });

                    services.AddSingleton<ISteamWorksCommunicator, SteamWorksCommunicator>();
                });
    }
}

