# HOK Beta Addin Manager

#### This tool dynamically generates Ribbon Tab items based on user selection.

###### Release 2018.0.0.7

* Bug fix where ResourceDictionary was causing exceptions because of duplicate keys. It appears that it resurfaced again. This time I have removed the Window.Resources all together and used in-line resources for the grid. There is no way that they are duplicated keys across these resources.
* Added Feedback button tool. 
* Added margins around the icons.
* Moved the Auto update checkbox and made it checked by default.

###### Release 2018.0.0.6

* Bug fix where ResourceDictionary was causing exceptions because of duplicate keys.
* Fixed Release_2016 configuration.

###### Release 2018.0.0.3

* Bug fix where reading assembly file would cause the dll to be loaded into app domain and lock the file on sysvol. That in turn causes inability for me to swap the files etc. 

###### Release 2018.0.0.2

* Since Mission Control tool has things that subscribe to events, and dockable panels, it requires to be properly loaded in on startup
to not crash. To accomodate that all new installed tools, are disabled, until user restarts Revit. 
* Added new "sysvol" based location for the beta tools to speed up the process of mirroring. 
* Beta installer will also now compare the contents of the current temp directory against what is installed in the user addins directory to identify any `HOK.` plugins that might be discontinued and delete them. 
* Minor UI fixes.

###### Release 2018.0.0.1

* Total revamp of what Jinsol was doing. It's now using class attributes to extract all required information from each beta DLL
in order to generate the ribbon items and populate WPF UI. 
