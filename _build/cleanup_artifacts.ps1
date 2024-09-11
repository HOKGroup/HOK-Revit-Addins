$revitYear = $Env:BUILD_CONFIGURATION

$objFolder = ".\_artifacts\obj"
Remove-Item -Path $objFolder -Recurse

$addinFolder = ".\_artifacts\{0}\" -f $revitYear
$libraryFolder =  "{0}\HOK-Addin.bundle\Contents\" -f $addinFolder

# Get all files in the source folder that do not end with ".addin"
$files = Get-ChildItem $addinFolder | Where-Object {!$_.PSIsContainer -and $_.Name -notlike "*.addin"}

# Copy each file to the library folder
foreach ($file in $files) {
    Copy-Item $file.FullName -Destination $libraryFolder
}

# Code sign all HOK DLLs
$dlls = Get-ChildItem $libraryFolder -Filter "HOK.*.dll"

foreach ($dll in $dlls) {
   & "$PSScriptRoot\..\_postBuild\codeSigning.ps1" $dll.FullName
}
