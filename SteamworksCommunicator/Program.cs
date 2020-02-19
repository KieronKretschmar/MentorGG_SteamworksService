using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitCommunicationLib.Interfaces;
using RabbitCommunicationLib.Producer;
using RabbitCommunicationLib.Queues;
using RabbitCommunicationLib.TransferModels;
using System;

namespace SteamworksService
{
    /// <summary>
    /// 
    /// Requires environment variables: ["AMQP_URI", "AMQP_GATHERER_QUEUE", "AMQP_DEMOCENTRAL_QUEUE"]
    /// </summary>
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

                    services.AddSingleton<ISteamworksCommunicator, SteamworksCommunicator>();

                    // Create producer
                    var outConnection = new QueueConnection(
                        hostContext.Configuration.GetValue<string>("AMQP_URI"),
                        hostContext.Configuration.GetValue<string>("AMQP_DEMOCENTRAL_QUEUE"));
                    services.AddSingleton<IProducer<GathererTransferModel>>(sp =>
                    {
                        return new Producer<GathererTransferModel>(outConnection);
                    });

                    // Create consumer
                    var inConnection = new QueueConnection(
                        hostContext.Configuration.GetValue<string>("AMQP_URI"),
                        hostContext.Configuration.GetValue<string>("AMQP_GATHERER_QUEUE"));
                    services.AddHostedService<GathererConsumer>(sp =>
                    {
                        return new GathererConsumer(inConnection, sp.GetService<ILogger<GathererConsumer>>(), sp.GetService<IProducer<GathererTransferModel>>(), sp.GetService<ISteamworksCommunicator>());
                    });
                });
    }
}

