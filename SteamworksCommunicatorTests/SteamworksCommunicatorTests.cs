using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingCodeGatherer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteamWorksAPITests
{
    [TestClass]
    public class SteamworksCommunicatorTests
    {
        private readonly ServiceProvider serviceProvider;

        public SteamworksCommunicatorTests()
        {

            var services = new ServiceCollection();
            services.AddSingleton<ISteamWorksCommunicator, SteamWorksCommunicator>();

            services.AddLogging(o =>
            {
                o.AddConsole();
                o.AddDebug();
            }); 
            serviceProvider = services.BuildServiceProvider();
        }


        [DataRow("CSGO-aCe7E-N7Wwp-wTHHr-JxUCi-kOijD", "http://replay190.valve.net/730/003380411038892556351_0861030327.dem.bz2")]
        [DataRow("CSGO-S7uAX-V2nVK-yriF9-wBZc8-hd5yD", "http://replay186.valve.net/730/003380416725429256317_0776860485.dem.bz2")]
        [DataRow("CSGO-iNYzz-7e2Fo-7K8in-bN3cp-LsXfE", "http://replay185.valve.net/730/003380422338951512163_1280347919.dem.bz2")]
        [DataRow("CSGO-473Bb-jST3U-TuN24-2TO5n-a3acP", "http://replay131.valve.net/730/003380524039482114620_1777201571.dem.bz2")]
        [DataRow("CSGO-MvtcC-omWiv-xsrDH-em7N9-6pe8E", "http://replay187.valve.net/730/003380546830726070786_1756050811.dem.bz2")]
        [DataRow("CSGO-j75zk-hw9ZA-mVWrG-bWZmR-AmhvN", "http://replay182.valve.net/730/003380560782927331612_1945900932.dem.bz2")]
        [DataRow("CSGO-oWGQF-d39x9-8Yxiz-eL5CY-kkAUM", "http://replay195.valve.net/730/003380933150149443663_1519731312.dem.bz2")]
        [DataRow("CSGO-XW5yE-8iuvD-uhtsU-cHBWB-UUYkF", "http://replay136.valve.net/730/003380957573480972327_0828287162.dem.bz2")]
        [DataRow("CSGO-4afPT-yKSFw-qcuKq-ZHSuE-zT2JE", "http://replay138.valve.net/730/003380964417511358513_0407517245.dem.bz2")]
        [DataRow("CSGO-AmJxH-8ZcxO-ibYpA-d5u8y-RBUWJ", "http://replay131.valve.net/730/003380566808766447905_1552731892.dem.bz2")]
        [DataRow("CSGO-XD9j9-RcCDN-eAvJN-xp7Or-7h3RG", "http://replay131.valve.net/730/003380587566343389436_0622180270.dem.bz2")]
        [DataRow("CSGO-jQedi-EtbjX-Qd87x-osN9e-pPocG", "http://replay135.valve.net/730/003380593643722113084_0956872480.dem.bz2")]
        [DataRow("CSGO-MCxS9-Q7GVw-8qqSn-tRu7C-od5yD", "http://replay132.valve.net/730/003380599946586620035_0872683108.dem.bz2")]
        [DataRow("CSGO-MoPUK-iPvDH-QzX8B-6d8m2-vLJjC", "http://replay183.valve.net/730/003381096921547407829_0711886860.dem.bz2")]
        [DataRow("CSGO-qa4uP-CmE88-KMJ3D-6wnMK-hJQBD", "http://replay185.valve.net/730/003380634654217338988_1811026012.dem.bz2")]
        [DataRow("CSGO-zadwr-Nz7Lb-TcnmS-y8Tbr-oNBYN", "http://replay136.valve.net/730/003380712199851868366_0354182261.dem.bz2")]
        [DataTestMethod]
        public async Task TranslateSharingCode(string sharingCode, string expectedUrl)
        {
            var steamId = (long)new Random().Next(1, 99999999);
            var swComm = serviceProvider.GetService<ISteamWorksCommunicator>();
            var matchData = await swComm.GetMatchData(steamId, sharingCode);
            Assert.AreEqual(matchData.DownloadUrl, expectedUrl);
        }

        [TestMethod]
        public async Task TranslateMultipleSharingCodes()
        {
            var sharingCodeUrlPairs = new Dictionary<string, string>()
            {
                {"CSGO-aCe7E-N7Wwp-wTHHr-JxUCi-kOijD", "http://replay190.valve.net/730/003380411038892556351_0861030327.dem.bz2"},
                {"CSGO-S7uAX-V2nVK-yriF9-wBZc8-hd5yD", "http://replay186.valve.net/730/003380416725429256317_0776860485.dem.bz2"},
                {"CSGO-iNYzz-7e2Fo-7K8in-bN3cp-LsXfE", "http://replay185.valve.net/730/003380422338951512163_1280347919.dem.bz2"},
                {"CSGO-473Bb-jST3U-TuN24-2TO5n-a3acP", "http://replay131.valve.net/730/003380524039482114620_1777201571.dem.bz2"},
                {"CSGO-MvtcC-omWiv-xsrDH-em7N9-6pe8E", "http://replay187.valve.net/730/003380546830726070786_1756050811.dem.bz2"},
                {"CSGO-j75zk-hw9ZA-mVWrG-bWZmR-AmhvN", "http://replay182.valve.net/730/003380560782927331612_1945900932.dem.bz2"},
                {"CSGO-oWGQF-d39x9-8Yxiz-eL5CY-kkAUM", "http://replay195.valve.net/730/003380933150149443663_1519731312.dem.bz2"},
                {"CSGO-XW5yE-8iuvD-uhtsU-cHBWB-UUYkF", "http://replay136.valve.net/730/003380957573480972327_0828287162.dem.bz2"},
                {"CSGO-4afPT-yKSFw-qcuKq-ZHSuE-zT2JE", "http://replay138.valve.net/730/003380964417511358513_0407517245.dem.bz2"},
                {"CSGO-AmJxH-8ZcxO-ibYpA-d5u8y-RBUWJ", "http://replay131.valve.net/730/003380566808766447905_1552731892.dem.bz2"},
                {"CSGO-XD9j9-RcCDN-eAvJN-xp7Or-7h3RG", "http://replay131.valve.net/730/003380587566343389436_0622180270.dem.bz2"},
                {"CSGO-jQedi-EtbjX-Qd87x-osN9e-pPocG", "http://replay135.valve.net/730/003380593643722113084_0956872480.dem.bz2"},
                {"CSGO-MCxS9-Q7GVw-8qqSn-tRu7C-od5yD", "http://replay132.valve.net/730/003380599946586620035_0872683108.dem.bz2"},
                {"CSGO-MoPUK-iPvDH-QzX8B-6d8m2-vLJjC", "http://replay183.valve.net/730/003381096921547407829_0711886860.dem.bz2"},
                {"CSGO-qa4uP-CmE88-KMJ3D-6wnMK-hJQBD", "http://replay185.valve.net/730/003380634654217338988_1811026012.dem.bz2"},
                {"CSGO-zadwr-Nz7Lb-TcnmS-y8Tbr-oNBYN", "http://replay136.valve.net/730/003380712199851868366_0354182261.dem.bz2" },
            };
            var steamId = (long)new Random().Next(1, 99999999);
            var swComm = serviceProvider.GetService<ISteamWorksCommunicator>();

            var tasks = new List<Task>();
            foreach (var sharingCode in sharingCodeUrlPairs.Keys)
            {
                tasks.Add(swComm.GetMatchData(steamId, sharingCode));
            }
            tasks.ForEach(x => x.Start());
            Task.WaitAll(tasks.ToArray());
        }
    }
}
