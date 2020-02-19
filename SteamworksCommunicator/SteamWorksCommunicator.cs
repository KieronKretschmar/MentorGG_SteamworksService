using Microsoft.Extensions.Logging;
using SteamworksService;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamworksService
{
    public interface ISteamworksCommunicator
    {
        SteamworksData GetMatchData(string sharingCode);
    }

    /// <summary>
    /// Communicates with SteamworksConnection via pipes to get match related data from sharingcodes.
    /// 
    /// To guarantee 1-by-1 execution of GetMatchData(), making the method synchronous and using a lock seems to be necessary. 
    /// Whether this service is added with AddTransient or AddSingleton to services does not seem to matter.
    /// </summary>
    public class SteamworksCommunicator : ISteamworksCommunicator
    {
        private readonly ILogger<ISteamworksCommunicator> _logger;
        private static readonly Object obj = new Object();

        public SteamworksCommunicator(ILogger<ISteamworksCommunicator> logger)
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
                using (NamedPipeClientStream pipeClient =
                    new NamedPipeClientStream(".", "ShareCodePipe", PipeDirection.InOut))
                {

                    // Connect to the pipe or wait until the pipe is available.
                    pipeClient.Connect();

                    using (StreamReader sr = new StreamReader(pipeClient))
                    using (StreamWriter sw = new StreamWriter(pipeClient))
                    {
                        _logger.LogInformation($"Writing SharingCode [ {sharingCode} ] to pipe.");
                        var pipeMessage = SharingCodeDecoded.FromSharingCode(sharingCode).ToPipeFormat();
                        sw.WriteLine(pipeMessage);
                        sw.Flush();

                        var response = sr.ReadLine();

                        _logger.LogInformation($"Received response [ {response} ] for [ {sharingCode} ].");
                        var demo = DecodeResponse(response);

                        _logger.LogInformation($"Decoded response yielding url [ {demo.DownloadUrl} ] and matchdate [ {demo.MatchDate} ] for [ {sharingCode} ].");
                        return demo;
                    }
                }
            }
        }

        private SteamworksData DecodeResponse(string response)
        {

            if (response.Substring(0, 6) == "--demo")
            {
                try
                {
                    var model = new SteamworksData();
                    var sections = response.Substring(7).Split('|');
                    model.DownloadUrl = sections[0];

                    var timestamp = long.Parse(sections[1]);
                    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    model.MatchDate = origin.AddSeconds(timestamp);
                    return model;
                }
                catch (Exception e)
                {
                    var msg = $"Error decoding response: {response}";
                    throw new DecodeResponseFailedException(msg, e);
                }
            }
            else
            {
                var msg = $"Response did not start with --demo. Response: {response}";
                throw new DecodeResponseFailedException();
            }
        }

        public struct SharingCodeDecoded
        {
            public UInt64 MatchId;
            public UInt64 OutcomeId;
            public UInt16 Token;

            public string ToPipeFormat()
            {
                return String.Format("{0}|{1}|{2}", MatchId, OutcomeId, Token);
            }

            public static SharingCodeDecoded FromSharingCode(string sc)
            {
                try
                {
                    const string DICTIONARY = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefhijkmnopqrstuvwxyz23456789";

                    //trim 'CSGO' and all the dashes to prepare base57 decode
                    if (sc.StartsWith("CSGO"))
                    {
                        sc = sc.Substring(4);
                    }
                    sc = sc.Replace("-", "");

                    BigInteger num = BigInteger.Zero;
                    foreach (var c in sc.ToCharArray().Reverse())
                    {
                        num = BigInteger.Multiply(num, DICTIONARY.Length) + DICTIONARY.IndexOf(c);
                    }

                    var data = num.ToByteArray().ToArray();

                    //unsigned fix
                    if (data.Length == 2 * sizeof(UInt64) + sizeof(UInt16))
                    {
                        data = data.Concat(new byte[] { 0 }).ToArray();
                    }

                    data = data.Reverse().ToArray();

                    SharingCodeDecoded result = new SharingCodeDecoded();

                    result.MatchId = BitConverter.ToUInt64(data, 1);
                    result.OutcomeId = BitConverter.ToUInt64(data, 1 + sizeof(UInt64));
                    result.Token = BitConverter.ToUInt16(data, 1 + 2 * sizeof(UInt64));

                    return result;

                }
                catch (Exception e)
                {
                    throw new DecodeSharingCodeFailedException($"Error Decoding SC {sc}", e);
                }
            }
        }

    }
}
