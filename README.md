# HOK-Revit-Addins 2024.1.0.22

### Code Signing

Code signing is an optional process that developers can choose to enable when building this repo. Autodesk strongly recommends that all Revit Plugins be signed with a code signing certificate, authenticating the plugin and adding its creator to the list of certified developers on users computer. This prevents a pop-up window from appearing every time user starts Revit, and tries to load a plugin from an unknown developer. Since HOK uses domain based certificates, if you are a developer for HOK, you would have (or can request to have it added by the IT) a certificate called "Code Signing DTM" installed on your machine. If that's the case simply comment out this line of code in the \*.csproj file:

`<Exec Command="&quot;C:\Program Files (x86)\Windows Kits\10\bin\10.0.17134.0\x64\signtool.exe&quot; sign /c &quot;Code Signing - DTM&quot; /v &quot;$(TargetPath)&quot;" />`

However, if you are one of the Open Source users of these tools, and want to use a specific PFX file located on your drive, and don't want to share your password with everyone use this code instead:

`<Exec Command="&quot;C:\Program Files (x86)\Windows Kits\10\bin\10.0.15063.0\x86\signtool.exe&quot; sign /f &quot;$(SolutionDir)..\_cert\archilabCertificate.pfx&quot; /p &quot;$([MSBuild]::GetRegistryValueFromView('HKEY_LOCAL_MACHINE\SOFTWARE\HOK', 'certificatePassword', null, RegistryView.Registry64, RegistryView.Registry32))&quot; /t http://timestamp.comodoca.com/authenticode &quot;$(TargetPath)&quot;" />`

For the above:

-   please make sure to replace the path to version of signtool.exe that you have available. These vary based on .NET version installed on your machine.
-   please make sure to replace the PFX file name to match one on your machine.
-   if you don't want to expose your password please create a new Registry key called "certificatePassword" under "HKEY_LOCAL_MACHINE\SOFTWARE\HOK" and store your password there. The above bit of code will make sure to read in at runtime.
-   potentially, depending on who's the provider of your certificate you might want to replace the path to authentication server.
-   last, but not least, please make sure to PLACE a copy of your certifacte in the roof folder of this repo under `_cert` folder. It will be ignored by the `.gitignore` so no worries about it getting published.

### Settings

Most of the tools in this repository have a reference to HOK.Core. This is because most of these tools use some utilities from that DLL. One of these utilities is a HOK Feedback tool that can be easily embedded into any other tool via a button. This tool allows users to post an issue directly to GitHub via a proxy user and a special access token. In order to make that tool work for anyone, anywhere, without distributing other people's tokens and repo info, there is a `Settings.json` file embedded into the Resources in HOK.Core.dll. Here's the contents schema:

```
{
  "FeedbackToken": "<access token>",
  "FeedbackPath": "/repos/<user>/<repo name>/",
  "ModelReportingServiceEndpoint": "<reporting server endpoint>",
  "CitrixDesktopConnectorKey": "<registry key path>",
  "CitrixDesktopConnectorValue": "<registry value name>",
  "FileOnOpeningFmeUserId": "<fme user id>",
  "FileOnOpeningFmePassword": "<fme password>",
  "FileOnOpeningFmeHost": "<fme endpoint>",
  "FileOnOpeningFmePort": <port number>,
  "FileOnOpeningFmeClientId": "<fme client id>",
  "HttpAddress": "<address to MC production server>",
  "HttpAddressDebug": "<address to MC testing server>"
  "ClarityUserId": "<Clarity user id>",
  "ClarityToken": "<Clarity access token>",
  "ClarityMachine": "<Machine used to generate access token>",
  "ClarityServers": ["<clarity_server1>", "<clarity_server2>"]
}
```

-   FeedbackToken - this is a token that you can obtain from GitHub to allow remote access to a repository. Since the purpose of this tool is to create GitHub issues on our behalf, we need an access token from authorized user, to login as that user and create a new issue. Replace this with a valid access token.
-   FeedbackPath - this is a url address to GitHub repository that you want to post issues to. In our case the address is `/repos/HOKGroup/MissionControl_Issues/`. Please not the `/repos/` and `/` at the end of the address. Make sure to replace the content of `<>` with a valid name, and make sure that access token is valid to grant user access to the repo.

Actual values can be found here: [HOK Teams](https://teams.microsoft.com/l/channel/19%3Aabfa34dfc38e4be68b6a26beed0bf8a1%40thread.skype/tab%3A%3A0cb3a662-0640-434e-af69-187825fcbe30)

### Links to release notes

1. <b>On Opening Monitor:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/tree/master/Utility%20Tools/src/HOK.FileOnpeningMonitor)
2. <b>Mission Control:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/HOK.MissionControl/README.md)
3. <b>HOK Core:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/HOK.Core/README.md)
4. <b>Beta Tools Manager:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/HOK.BetaToolsManager/README.md)
5. <b>Element Mover:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/Utility%20Tools/src/HOK.ElementMover/Readme.md)
6. <b>Mass Tools:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/Mass%20Tools/README.MD)
7. <b>Move Backup:</b> [Tool Page and Release notes.](https://github.com/HOKGroup/HOK-Revit-Addins/blob/master/HOK.MoveBackup/README.md)

### Tips

<li> Always run Visual Studio in Administrator mode. Otherwise it's possible that post build events will fail.
