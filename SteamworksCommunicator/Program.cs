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

                    #region Enviroment Variable Checks
                    // Confirm requirements are met
                    var AMQP_URI = hostContext.Configuration.GetValue<string>("AMQP_URI");
                    if (AMQP_URI == null)
                        throw new ArgumentException("AMQP_URI is missing, configure the `AMQP_URI` enviroment variable!");

                    var AMQP_DEMOCENTRAL_QUEUE = hostContext.Configuration.GetValue<string>("AMQP_DEMOCENTRAL_QUEUE");
                    if (AMQP_DEMOCENTRAL_QUEUE == null)
                        throw new ArgumentException("AMQP_DEMOCENTRAL_QUEUE is missing, configure the `AMQP_DEMOCENTRAL_QUEUE` enviroment variable!");

                    var AMQP_GATHERER_QUEUE = hostContext.Configuration.GetValue<string>("AMQP_GATHERER_QUEUE");
                    if (AMQP_GATHERER_QUEUE == null)
                        throw new ArgumentException("AMQP_GATHERER_QUEUE is missing, configure the `AMQP_GATHERER_QUEUE` enviroment variable!");


                    Console.WriteLine("Environment: ");
                    Console.WriteLine($"AMQP_URI: [ {AMQP_URI} ]");
                    Console.WriteLine($"AMQP_DEMOCENTRAL_QUEUE: [ {AMQP_DEMOCENTRAL_QUEUE} ]");
                    Console.WriteLine($"AMQP_GATHERER_QUEUE: [ {AMQP_GATHERER_QUEUE} ]");

                    #endregion


                    // Create producer
                    var outConnection = new QueueConnection(
                        AMQP_URI,
                        AMQP_DEMOCENTRAL_QUEUE);
                    services.AddSingleton<IProducer<GathererTransferModel>>(sp =>
                    {
                        return new Producer<GathererTransferModel>(outConnection);
                    });

                    // Create consumer
                    var inConnection = new QueueConnection(
                        AMQP_URI,
                        AMQP_GATHERER_QUEUE);
                    services.AddHostedService<GathererConsumer>(sp =>
                    {
                        return new GathererConsumer(inConnection, sp.GetService<ILogger<GathererConsumer>>(), sp.GetService<IProducer<GathererTransferModel>>(), sp.GetService<ISteamworksCommunicator>());
                    });
                });
    }
}

