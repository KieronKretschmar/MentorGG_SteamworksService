# SteamworksService
Consumes SharingCodes and other matchupload related data, enriches it with information like download urls provided by Steamworks, and publishes the enriched data to a queue.

## Build
Needs to run in the same container as [SteamworksConnectionLinux](https://gitlab.com/mentorgg/csgo/steamworksconnectionlinux)

## Environment variables
- `AMQP_URI` : RabbitMQ Instance URI [\*]
- `AMQP_GATHERER_QUEUE` : Queue to consume from, should be connected to SharingCodeGatherer. [\*]
- `AMQP_DEMOCENTRAL_QUEUE` : Queue to publish to, should be connected to DemoCentral. [\*]

