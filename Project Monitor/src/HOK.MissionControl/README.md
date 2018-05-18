# HOK Mission Control .NET

#### The .NET side of Mission Control and how it integrates with Revit. It contains multiple libraries: 

* HOK.MissionControl.dll - main library with all events handled, IUpdaters as well dockable panels created. 
* HOK.MissionControl.Core.dll - core server interaction library. Contains all methods for sending and getting data from MongoDB
* HOK.MissionControl.FamilyPublish.dll - External Command for publishing Family information to Health Monitor part of Mission Control.
* HOK.MissionControl.LinksManager.dll - External Command for managing links, imports and styles. 

###### Release 2018.0.1.01

* Added export functionality for Groups. That includes data publishing as well health report summary tab. 

###### Release 2018.0.1.00

* Major data base refactor. 
* All database calls were moved to be on background threads. This allows us to open the model without waiting for DB to accept all of our requests. It also moves all data posting to DocumentOpened and DocumentSynched events rather then on DocumentClosing. 
* Implemented additional user overrides for family names and styles. 
* Implemented a method for closing all tasks windows when Revit document is closed. 
* Implemented a method for closing Socket channel when Revit document is closed so that we don't subscribe to it multipe times.
* MC buttons on Revit ribbon will be now disabled unless there was a successful connection to MC. 
* Data can now be posted to MC right away. There is no need to restart Revit anymore (that still needs to be addressed for tasks).
* Bug fixes to Family publish where data about in place families was not published. 
* Many more....

###### Release 2018.0.0.20

* Fixed a bug in how Model Size was being obtained so that BIM 360 files are also covered here. It uses cached file size which is not always 100% accurate but it's good for monitoring changes.  

###### Release 2018.0.0.19

* Added support for BIM 360 files so that they can be added to Mission Control.

###### Release 2018.0.0.18

* Use asynch methods to publish plug-in info. That speeds up UI display, and finishes publishing in the background.

###### Release 2018.0.0.17

* As Mark reported we were not storing date and time in MongoDB using UTC time, so depending on user location some dates might be all over the place. This fixes that issue making sure all timestamps are always in UTC. 
* Converted all Health Report publish methods to be executed at startup and are now using new Thread(). This should speed things up and free up post processing when model is being closed. This will also prevent any data being published to DB that was not saved. We are no longer publishing anything onDocumentClosing()
* Removed data that was being published about Session Logs etc. It was not being used by Mission Control and was clogging up the pipeline for no reason. It should speed things up a little.

###### Release 2018.0.0.16

* Adds info about views to health report dimension segments
* Fixes a bug where dimensions that had no segments were skipped.
* Adds user overrides from configuration to dimension styles overrides

###### Release 2018.0.0.15

* Added ability to use user overrides stored in updaters to retrieve all strings required for family name checks. 
* Added a fix to dimension class wrappers that were failing due to value being stored as string originally and later changed to be double. These are potentially API breaking changes. 
* Added fix to dictionaries storing configurations to use case insensitive key lookup since all paths are now stored as lowere case. 

###### Release 2018.0.0.14

* Fix to Family Task window and how text was wrappinh. 
* Small easthetic upgrades to improve spacing.
* Fixed bug where comments were getting assigned value from the message. 

###### Release 2018.0.0.13

* Small bug fixes to Dimension Styles and how units are converted.

###### Release 2018.0.0.12

* Adds Styles to Health Report. That includes updates to Communicator tool to get styles graphics up on the dockable window, as well as new methods to collect and publish Styles info from the model. 
* Adds Status Reported tool. It's a small WPF utility window that pops up a 100 X 50 px message bubble in the bottom/right corner of the screen. It automatically dissapears after 5s. For now it's reporting status of the Styles getting posted to DB, but later it can be used for other purposes. 
* Adds ability to post styles Async from another thread. To do that we had to start a new thread, and to integrate Status Reporter there was an extension added to External Event Handler for Revit. 
* Fixed small bug in the Health Report data collection where it was getting the wrong data set for Model Stats. 

###### Release 2018.0.0.11

* ?

###### Release 2018.0.0.10

* Fixed a bug where central file path for health records and sheets was not getting published with lower case. That caused issues with it not getting properly recognized on server side.

###### Release 2018.0.0.9

* Enhanced Website Link Tool to navigate to project if possible, rather than always navigating to home page.

###### Release 2018.0.0.8

* Fixed the issue with DTM where DTM Tool would show a warning on Synch to Central warning users of modifying View Templates.

###### Release 2018.0.0.7

* Remove obsolete PostToMongo class. 
* Add user info to data posted for model open times, model synch times, session info etc.

###### Release 2018.0.0.6

* Fix to a bug in Server Utilities where file path was not generated properly. It led to wrong file being returned from DB.

###### Release 2018.0.0.5

* Actually connected and release Family Tasks. 
* Also added Sheet Tasks and Sheet management tool.
* Offloaded all tasks related stuff to another thread and integrated with Socket.io. 
* New Button on the ribbon linking back to Mission Control website.
* Bunch of bug fixes.

###### Release 2018.0.0.4

* Added Tasks to Communicator Tab.
* Messaging integration for Tasks form and MongoDB using Sockets.IO.
* New class decorators for serialization and descerialization of MongoDB data.
* Addins now publish user location.
* Cancel button for Family Publish.
* More robust handling of failures while exporting Families.
* Family export logic for updating rather than constantly adding new items. Should improve server response time with smaller paylods.


###### Release 2018.0.0.3

* Fix to Communicator button not always activating the docable panel. 
* Added Communicator icon that changes colors to indicate Communicator state.

###### Release 2018.0.0.2

* Fix to Mission Control Single Session. It was replaced with Link Unload Monitor to better describe the original intent of preventing
users from unloading links. It hijacks Unload for All Users method in Revit, replacing it with our own message. 
* Fix to DTM Tool where it was needlessly popping up on Synch and Reload events. Fix is provided by hijacking of Reload Latest command
with our own reload functionality that first disables DTM Tool. Similar apprach is implemented for Synch functionality. 
* Fix to DTM Tool that was preventing users from creating new Grids, Links, Levels etc. and editing them in the same session of Revit. 
Instead it will now allow users to create them and edit, only upon closing and reopening will they be added to "monitored" elements list
and users will be stopped from messing with it. 
* Addition of Communicator tool, that displays a summary of Health Report in a dockable panel.
* Family Publish tool adds additional properties to its export. Things such as number of reference planes, voids, extrusions, etc. 
* Other small fixes and refactors.

###### Release 2018.0.0.1

* Update to 2018
* Added Logging functionality
* Major code cleanup and refactor
