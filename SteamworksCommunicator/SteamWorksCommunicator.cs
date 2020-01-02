using Microsoft.Extensions.Logging;
using RabbitTransfer.TransferModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharingCodeGatherer
{
    public interface ISteamWorksCommunicator
    {
        SteamworksData GetMatchData(string sharingCode);
    }

    /// <summary>
    /// Communicates with SteamworksConnection via pipes to get match related data from sharingcodes.
    /// 
    /// To guarantee 1-by-1 execution of GetMatchData(), making the method synchronous and using a lock seems to be necessary. 
    /// Whether this service is added with AddTransient or AddSingleton to services does not seem to matter.
    /// </summary>
    public class SteamWorksCommunicator : ISteamWorksCommunicator
    {
        private const string pipeNameOut = "/tmp/swcpipei";
        private const string pipeNameIn = "/tmp/swcpipeo";
        private readonly ILogger<ISteamWorksCommunicator> _logger;
        private static readonly Object obj = new Object();

        public SteamWorksCommunicator(ILogger<ISteamWorksCommunicator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Writes the sharingCode to pipe and returns a SteamworksData object based on the response.
        /// 
        /// Throws Exceptions: TimeoutException
        /// </summary>
        /// <param name="steamId"></param>
        /// <param name="sharingCode"></param>
        /// <returns></returns>
        public SteamworksData GetMatchData(string sharingCode)
        {
            lock (obj)
            {
                using (NamedPipeClientStream pipeOut = new NamedPipeClientStream(".", pipeNameOut, PipeDirection.Out))
                {
                    try
                    {
                        pipeOut.Connect(1000);
                    }
                    catch (TimeoutException e)
                    {
                        _logger.LogError($"Could not connect to {pipeNameOut}", e);
                        throw;
                    }

                    using (StreamWriter sw = new StreamWriter(pipeOut))
                    {
                        sw.WriteLine(sharingCode);
                        sw.Flush();
                    }
                }

                using (NamedPipeClientStream pipeIn = new NamedPipeClientStream(".", pipeNameIn, PipeDirection.In))
                {
                    try
                    {
                        pipeIn.Connect(1000);
                    }
                    catch (TimeoutException e)
                    {
                        _logger.LogError($"Could not connect to {pipeNameIn}", e);
                        throw;
                    }
                    using (StreamReader sr = new StreamReader(pipeIn))
                    {
                        var response = sr.ReadLine();

                        TryDecodeResponse(response, out var demo);
                        return demo;
                    }
                }
            }
        }

        private bool TryDecodeResponse(string response, out SteamworksData model)
        {
            model = new SteamworksData();
            try
            {
                if (response.Substring(0, 6) == "--demo")
                {
                    var sections = response.Substring(7).Split('|');
                    model.DownloadUrl = sections[0];

                    var timestamp = long.Parse(sections[1]);
                    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    model.MatchDate = origin.AddSeconds(timestamp);
                    return true;
                }
                _logger.LogInformation($"Response did not start with --demo. Response: {response}");
                return false;
            }
            catch (Exception e)
            {
                model = new SteamworksData();
                _logger.LogError($"Error decoding response: {response}", e);
                return false;
            }
        }
    }
}
