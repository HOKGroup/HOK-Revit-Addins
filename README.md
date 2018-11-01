HOK-Revit-Addins 2019.0.0.X
================

### Settings

Most of the tools in this repository have a reference to HOK.Core. This is because most of these tools use some utilities from that DLL. One of these utilities is a HOK Feedback tool that can be easily embedded into any other tool via a button. This tool allows users to post an issue directly to GitHub via a proxy user and a special access token. In order to make that tool work for anyone, anywhere, without distributing other people's tokens and repo info, there is a `Settings.json` file embedded into the Resources in HOK.Core.dll. Here's the contents schema: 
```
{
  "FeedbackToken": "<access token>",
  "FeedbackPath": "/repos/<user>/<repo name>/"
}
```
* FeedbackToken - this is a token that you can obtain from GitHub to allow remote access to a repository. Since the purpose of this tool is to create GitHub issues on our behalf, we need an access token from authorized user, to login as that user and create a new issue. Replace this with a valid access token. 
* FeedbackPath - this is a url address to GitHub repository that you want to post issues to. In our case the address is `/repos/HOKGroup/MissionControl_Issues/`. Please not the `/repos/` and `/` at the end of the address. Make sure to replace the content of `<>` with a valid name, and make sure that access token is valid to grant user access to the repo. 

Actual values can be found here: [HOK Teams](https://hok365.sharepoint.com/:u:/s/DesignTechSoftwareDevelopment/EfJpKQL6fp1Cn6S5NSDIGOIBue1moWdwEOVex_wo6Z5nzw?e=2BSURC)

### Links to release notes

1. <b>On Opening Monitor:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/tree/master/Utility%20Tools/src/HOK.FileOnpeningMonitor)
2. <b>Mission Control:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/Project%20Monitor/src/HOK.MissionControl/README.md)
3. <b>HOK Core:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/HOK.Core/README.md) 
4. <b>Beta Installer:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/HOK%20Beta%20Tools/README.md)
5. <b>Addins Installer:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/tree/master/HOK%20AddIns%20Installer/README.md)
6. <b>Element Mover:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/Utility%20Tools/src/HOK.ElementMover/Readme.md)
7. <b>Mass Tools:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/Mass%20Tools/README.MD)
8. <b>Move Backup:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/HOK.MoveBackup/README.md)

### Tips
<li> Always run Visual Studio in Administrator mode. Otherwise it's possible that post build events will fail.
