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
                    services.AddLogging(services =>
                    {
                        services.AddConsole(o =>
                        {
                            o.TimestampFormat = "[yyyy-MM-dd HH:mm:ss zzz] ";
                        });
                        services.AddDebug();
                    });

                    services.AddSingleton<ISteamworksCommunicator, SteamworksCommunicator>();

                    #region Enviroment Variables
                    var AMQP_URI = GetRequiredEnvironmentVariable<string>(hostContext.Configuration, "AMQP_URI");
                    var AMQP_DEMOCENTRAL_QUEUE = GetRequiredEnvironmentVariable<string>(hostContext.Configuration, "AMQP_DEMOCENTRAL_QUEUE");
                    var AMQP_GATHERER_QUEUE = GetRequiredEnvironmentVariable<string>(hostContext.Configuration, "AMQP_GATHERER_QUEUE");
                    var AMQP_PREFETCH_COUNT = GetOptionalEnvironmentVariable<ushort>(hostContext.Configuration, "AMQP_PREFETCH_COUNT", 0);

                    Console.WriteLine("Environment: ");
                    Console.WriteLine($"AMQP_URI: [ {AMQP_URI} ]");
                    Console.WriteLine($"AMQP_DEMOCENTRAL_QUEUE: [ {AMQP_DEMOCENTRAL_QUEUE} ]");
                    Console.WriteLine($"AMQP_GATHERER_QUEUE: [ {AMQP_GATHERER_QUEUE} ]");
                    Console.WriteLine($"AMQP_PREFETCH_COUNT: [ {AMQP_PREFETCH_COUNT} ]");
                    #endregion


                    // Create producer
                    var outConnection = new QueueConnection(
                        AMQP_URI,
                        AMQP_DEMOCENTRAL_QUEUE);
                    services.AddSingleton<IProducer<DemoInsertInstruction>>(sp =>
                    {
                        return new Producer<DemoInsertInstruction>(outConnection);
                    });

                    // Create consumer
                    var inConnection = new QueueConnection(
                        AMQP_URI,
                        AMQP_GATHERER_QUEUE);
                    services.AddHostedService<GathererConsumer>(sp =>
                    {
                        return new GathererConsumer(
                            inConnection,
                            sp.GetService<ILogger<GathererConsumer>>(),
                            sp.GetService<IProducer<DemoInsertInstruction>>(),
                            sp.GetService<ISteamworksCommunicator>(),
                            AMQP_PREFETCH_COUNT);
                    });
                });



        /// <summary>
        /// Attempt to retrieve an Environment Variable
        /// Throws ArgumentNullException is not found.
        /// </summary>
        /// <typeparam name="T">Type to retreive</typeparam>
        private static T GetRequiredEnvironmentVariable<T>(IConfiguration config, string key)
        {
            T value = config.GetValue<T>(key);
            if (value == null)
            {
                throw new ArgumentNullException(
                    $"{key} is missing, Configure the `{key}` environment variable.");
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Attempt to retrieve an Environment Variable
        /// Returns default value if not found.
        /// </summary>
        /// <typeparam name="T">Type to retreive</typeparam>
        private static T GetOptionalEnvironmentVariable<T>(IConfiguration config, string key, T defaultValue)
        {
            var stringValue = config.GetSection(key).Value;
            try
            {
                T value = (T)Convert.ChangeType(stringValue, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
                return value;
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine($"Env var [ {key} ] not specified. Defaulting to [ {defaultValue} ]");
                return defaultValue;
            }
        }
    }
}

