# HOK Mission Control .NET

#### The .NET side of Mission Control and how it integrates with Revit. It contains multiple libraries: 

* HOK.MissionControl.dll - main library with all events handled, IUpdaters as well dockable panels created. 
* HOK.MissionControl.Core.dll - core server interaction library. Contains all methods for sending and getting data from MongoDB
* HOK.MissionControl.FamilyPublish.dll - External Command for publishing Family information to Health Monitor part of Mission Control.
* HOK.MissionControl.LinksManager.dll - External Command for managing links, imports and styles. 

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
