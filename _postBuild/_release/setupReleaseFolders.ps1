param([string]$NewReleaseFolderName="NewRelease", [string]$OldReleaseFolderName="OldRelease")

if (Test-Path -Path $NewReleaseFolderName) {
	Write-Error "NewRelease folder already exists"
	exit 1
}
if ($(Test-Path -Path $OldReleaseFolderName) -eq $false) {
	Write-Error "Old folder does not exist"
	exit 1
}
mkdir $NewReleaseFolderName
mkdir "$NewReleaseFolderName\prev"
mkdir "$NewReleaseFolderName\master"
Copy-Item -Path "$OldReleaseFolderName\$OldReleaseFolderName\*" -Destination "$NewReleaseFolderName\prev" -Recurse -Exclude "*.zip"




