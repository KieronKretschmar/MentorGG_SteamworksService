# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

# [1.1.2]
### Changed
- Prefetch default to 1 (previously 0, which equals infinity)

# [1.1.1]
### Changed
- Prevent infinite loop on certain failures

# [1.1.0] - 2020-07-13
### Added
- AMQP_PREFETCH_COUNT env var
### Changed
- SteamworksCommunicator now waits 30s before trying write a failed message to pipe once again. 

## [1.0.0] - 2020-05-13
### Added
- Handling of response from SWC indicating Steam user is not logged in and treat it as a temporary exception resulting in sleep + resending.
### Changed
- GathererConsumer now sleeps for 1 second on temporary exception
- CI improvements
- Fix project references

## [0.1.0] - ?
### Added
- CI

###Changed
- Logging with correct timestamp