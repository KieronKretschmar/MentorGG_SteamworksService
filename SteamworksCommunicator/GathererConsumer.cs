using RabbitMQ.Client;
using RabbitTransfer.Consumer;
using RabbitTransfer.Interfaces;
using RabbitTransfer.TransferModels;
using SharingCodeGatherer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamworksCommunicator
{
    /// <summary>
    /// Consumer for messages from SharingCodeGatherer.
    /// Receives all the messages containing SharingCodes, adds information from Steamworks, and publishes them to DemoCentral.
    /// </summary>
    public class GathererConsumer : Consumer<SCG_SWS_Model>
    {
        private readonly IProducer<GathererTransferModel> _producer;
        private readonly ISteamWorksCommunicator _swComm;

        public GathererConsumer(IQueueConnection queueConnection, IProducer<GathererTransferModel> producer, ISteamWorksCommunicator swComm) : base(queueConnection)
        {
            _producer = producer;
            _swComm = swComm;
        }

        public override void HandleMessage(IBasicProperties properties, SCG_SWS_Model inboundModel)
        {
            // Get data from Steamworks
            SteamworksData swData;
            try
            {
                swData = _swComm.GetMatchData(inboundModel.SharingCode);
            }
            catch (TimeoutException e)
            {
                // TODO: Nack this so it gets requeued
                throw;
            }

            // Create outbound model
            var outboundModel = new GathererTransferModel
            {
                DownloadUrl = swData.DownloadUrl,
                MatchDate = swData.MatchDate,
                UploaderId = inboundModel.UploaderId,
                UploadType = inboundModel.UploadType,
                Source = RabbitTransfer.Enums.Source.Valve,
            };

            // Publish 
            _producer.PublishMessage(new Guid().ToString(), outboundModel);
        }
    }
}
