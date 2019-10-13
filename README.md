[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=maartenvana_sinance&metric=alert_status)](https://sonarcloud.io/dashboard?id=maartenvana_sinance)
![Docker Pulls](https://img.shields.io/docker/pulls/maartenvana/sinance)
![Docker Cloud Build Status](https://img.shields.io/docker/cloud/build/maartenvana/sinance)

# Sinance
Simple finances

# Please Note
This was a private project from 2014 and was started because of my discontent with existing "Home finance" solutions. This project runs fine and suits my needs but is in no way to be called "good software". It has recently been ported to .NET Core to be run inside of a container but there still is alot to do.

# To-do
A general list of things that really need doing, in order of necessity:

Version 2.0:
- Reorganize backend code in logical sections
  - Refactor code from controllers to services
- (re)Add unit tests
- Incorporate default (ING/ABN AMRO) import rules in the software
- Translate all dutch texts to english
- Create instructions to run the software

Version 3.0:
- Rebuild/update front-end
  - Update the javascript libraries
  - Split up big pages/sections to components?
- Add predictions/planning

Other If I ever run out of things to do:
- Multilanguage support
- Add support to import any kind of CSV/TXT file for transactions

All the details will be in the issue tracker
