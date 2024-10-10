# Usage
# .\_build\local_build.ps "2025" "Debug 25"

$ErrorActionPreference = 'Continue'
$revitVersion = $args[0]
$buildConfiguration = $args[1]
$env:BUILD_CONFIGURATION = $revitVersion
$sourceDir = $pwd


## Copy Resources to artifacts folder
& "$sourceDir/_build/copy_resources.ps1"

dotnet build "HOK.Core\HOK.Core.sln" -c "$buildConfiguration" --artifacts-path "$sourceDir/_artifacts" --property:OutputPath="$sourceDir/_artifacts/$revitVersion"

dotnet build "HOK.MissionControl\HOK.MissionControl.Core\HOK.MissionControl.Core.csproj" -c "$buildConfiguration" --artifacts-path "$sourceDir/_artifacts" --property:OutputPath="$sourceDir/_artifacts/$revitVersion"

dotnet build "HOK.MissionControl\HOK.MissionControl\HOK.MissionControl.csproj" -c "$buildConfiguration" --artifacts-path "$sourceDir/_artifacts" --property:OutputPath="$sourceDir/_artifacts/$revitVersion"

dotnet build "HOK.Feedback\HOK.Feedback.sln" -c "$buildConfiguration" --artifacts-path "$sourceDir/_artifacts" --property:OutputPath="$sourceDir/_artifacts/$revitVersion"

dotnet build "HOK.MissionControl\HOK.MissionControl.sln" -c "$buildConfiguration" --artifacts-path "$sourceDir/_artifacts" --property:OutputPath="$sourceDir/_artifacts/$revitVersion"

$solutions = Get-ChildItem . -Filter "HOK.*.sln" -Recurse -Depth 2 -Exclude HOK.MissionControl*,HOK.Core*,HOK.Feedback*,HOK.ParameterTools*,HOK.FileOpeningMonitor*,HOK.ModelReporting*

foreach ($solution in $solutions) {
    dotnet build $solution.FullName -c "$buildConfiguration" --artifacts-path "$sourceDir/_artifacts" --property:OutputPath="$sourceDir/_artifacts/$revitVersion"

}

& "$sourceDir/_build/copy_artifacts.ps1"

& "$sourceDir/_build/cleanup_artifacts.ps1"

Copy-Item ".\_artifacts\$revitVersion\HOK-Addin.bundle\Contents\*" "C:\ProgramData\Autodesk\Revit\Addins\$revitVersion\HOK-Addin.bundle\Contents\"

Remove-Item Env:\BUILD_CONFIGURATION