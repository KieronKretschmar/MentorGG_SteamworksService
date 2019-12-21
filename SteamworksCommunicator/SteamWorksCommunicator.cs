using Microsoft.Extensions.Logging;
using RabbitTransfer.TransferModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SharingCodeGatherer
{
    public interface ISteamWorksCommunicator
    {
        Task<GathererTransferModel> GetMatchData(long steamId, string sharingCode);
    }

    public class SteamWorksCommunicator : ISteamWorksCommunicator
    {
        private const string PipeNameOut = "/tmp/swcpipei";
        private const string PipeNameIn = "/tmp/swcpipeo";
        private readonly ILogger<ISteamWorksCommunicator> _logger;

        public SteamWorksCommunicator(ILogger<ISteamWorksCommunicator> logger)
        {
            _logger = logger;
        }

        public async Task<GathererTransferModel> GetMatchData(long steamId, string sharingCode)
        {
            throw new NotImplementedException();
        }
    }
}
