using Microsoft.Extensions.Logging;
using RabbitCommunicationLib.Consumer;
using RabbitCommunicationLib.Enums;
using RabbitCommunicationLib.Interfaces;
using RabbitCommunicationLib.TransferModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamworksService
{
    /// <summary>
    /// Consumer for messages from SharingCodeGatherer.
    /// Receives all the messages containing SharingCodes, adds information from Steamworks, and publishes them to DemoCentral.
    /// </summary>
    public class GathererConsumer : Consumer<SharingCodeInstruction>
    {
        const int RETRY_AFTER_MILLISECONDS = 1000;

        private readonly ILogger<GathererConsumer> _logger;
        private readonly IProducer<DemoInsertInstruction> _producer;
        private readonly ISteamworksCommunicator _swComm;

        public GathererConsumer(
            IQueueConnection queueConnection, 
            ILogger<GathererConsumer> logger, 
            IProducer<DemoInsertInstruction> producer, 
            ISteamworksCommunicator swComm,
            ushort prefetchCount) : base(queueConnection, prefetchCount)
        {
            _logger = logger;
            _producer = producer;
            _swComm = swComm;
        }

        public override async Task<ConsumedMessageHandling> HandleMessageAsync(BasicDeliverEventArgs ea, SharingCodeInstruction inboundModel)
        {
            try
            {
                _logger.LogInformation($"Received message with SharingCode [ {inboundModel.SharingCode} ].");

                // Get data from Steamworks
                var swData = _swComm.GetMatchData(inboundModel.SharingCode);

                // Create outbound model
                var outboundModel = new DemoInsertInstruction
                {
                    DownloadUrl = swData.DownloadUrl,
                    MatchDate = swData.MatchDate,
                    UploaderId = inboundModel.UploaderId,
                    UploadType = inboundModel.UploadType,
                    Source = RabbitCommunicationLib.Enums.Source.Valve,
                };

                _logger.LogInformation($"Successfully handled message with SharingCode [ {inboundModel.SharingCode} ].");

                _producer.PublishMessage(outboundModel);
                return ConsumedMessageHandling.Done;
            }
            // If it seems like a temporary failure, resend message
            catch (Exception e) when (e is TimeoutException || e is SteamNotLoggedInException)
            {
                _logger.LogWarning($"Message with SharingCode [ {inboundModel.SharingCode} ] could not be handled right now. " +
                    $"Instructing the message to be resent, and sleeping for [ {RETRY_AFTER_MILLISECONDS} ] ms, assuming this is a temporary failure.", e);
                Thread.Sleep(RETRY_AFTER_MILLISECONDS);

                return ConsumedMessageHandling.Resend;
            }
            // When in doubt or the message itself might be corrupt, throw away
            catch (Exception e)
            {
                _logger.LogError($"Message with SharingCode [ {inboundModel.SharingCode} ] could not be handled. Instructing the message to be thrown away, assuming the message is corrupt.", e);

                return ConsumedMessageHandling.ThrowAway;
            }
        }
    }
}
