using Microsoft.Extensions.Logging;
using RabbitCommunicationLib.Consumer;
using RabbitCommunicationLib.Interfaces;
using RabbitCommunicationLib.TransferModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteamworksService
{
    /// <summary>
    /// Consumer for messages from SharingCodeGatherer.
    /// Receives all the messages containing SharingCodes, adds information from Steamworks, and publishes them to DemoCentral.
    /// </summary>
    public class GathererConsumer : Consumer<SCG_SWS_Model>
    {
        private readonly ILogger<GathererConsumer> _logger;
        private readonly IProducer<GathererTransferModel> _producer;
        private readonly ISteamworksCommunicator _swComm;

        public GathererConsumer(IQueueConnection queueConnection, ILogger<GathererConsumer> logger, IProducer<GathererTransferModel> producer, ISteamworksCommunicator swComm) : base(queueConnection)
        {
            _logger = logger;
            _producer = producer;
            _swComm = swComm;
        }

        public override async Task HandleMessageAsync(BasicDeliverEventArgs ea, SCG_SWS_Model inboundModel)
        {
            try
            {
                // Get data from Steamworks
                var swData = _swComm.GetMatchData(inboundModel.SharingCode);

                // Create outbound model
                var outboundModel = new GathererTransferModel
                {
                    DownloadUrl = swData.DownloadUrl,
                    MatchDate = swData.MatchDate,
                    UploaderId = inboundModel.UploaderId,
                    UploadType = inboundModel.UploadType,
                    Source = RabbitCommunicationLib.Enums.Source.Valve,
                };

                // Publish 
                _producer.PublishMessage(new Guid().ToString(), outboundModel);
            }
            catch (DecodeSharingCodeFailedException e)
            {
                _logger.LogError($"Error handling message [ {inboundModel.ToJson()} ]", e);
            }
            catch (DecodeResponseFailedException e)
            {
                _logger.LogError($"Error handling message [ {inboundModel.ToJson()} ]", e);
            }
            catch (TimeoutException e)
            {
                _logger.LogError($"TimeoutException when handling message [ {inboundModel.ToJson()} ]. Steam Servers down?", e);
                // Requeue this message
                this.BasicNack(ea, true);
            }
        }
    }
}
