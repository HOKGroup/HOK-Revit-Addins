$regexToKeep = "^([.|_]\w+)"
$projectsToKeep = @(
    "AVF Manager",
    "Element Tools",
    "HOK.AddInManager",
    "HOK.BetaToolsManager",
    "HOK.Core",
    "HOK.Feedback",
    "HOK.MissionControl",
    "HOK.MoveBackup",
    "HOK.RibbonTab"
    "LPD Calculator",
    "Mass Tools",
    "Model Reporting",
    "Navigator",
    "Parameter Tools",
    "Sheet Manager",
    "Smart BCF",
    "Utility Tools",
)

foreach ($project in $projectsToKeep) {
    $regexToKeep += "|" + $project
}

Get-ChildItem -Path "$Env:BUILD_SOURCESDIRECTORY" -Directory |
 Where-Object { $_.Name -notMatch "$regexToKeep" } |
 ForEach-Object { Remove-Item -Recurse -LiteralPath $_.FullName }

Get-ChildItem -Path "$Env:BUILD_SOURCESDIRECTORY\AVF Manager\src" -Directory |
 Where-Object { $_.Name -notMatch "HOK.ViewAnalysis$" } |
 ForEach-Object { Remove-Item -Recurse -LiteralPath $_.FullName }

Get-ChildItem -Path "$Env:BUILD_SOURCESDIRECTORY\Mass Tools\src" -Directory |
 Where-Object { $_.Name -notMatch "HOK.RoomsToMass_DirectShape" } |
 ForEach-Object { Remove-Item -Recurse -LiteralPath $_.FullName }

Get-ChildItem -Path "$Env:BUILD_SOURCESDIRECTORY\Navigator\src" -Directory |
 Where-Object { $_.Name -notMatch "HOK.Navigator$" } |
 ForEach-Object { Remove-Item -Recurse -LiteralPath $_.FullName }

Get-ChildItem -Path "$Env:BUILD_SOURCESDIRECTORY\Sheet Manager\src\Archive" -Directory |
 Where-Object { $_.Name -notMatch "HOK.SheetManager_Excel$" } |
 ForEach-Object { Remove-Item -Recurse -LiteralPath $_.FullName }

Remove-Item -Recurse -LiteralPath "$Env:BUILD_SOURCESDIRECTORY\Sheet Manager\src\HOK.SheetManager"

Get-ChildItem -Path "$Env:BUILD_SOURCESDIRECTORY\Utility Tools\src" -Directory |
 Where-Object { $_.Name -notMatch "(HOK.ElementFlatter|HOK.ElementMover|HOK.FileOnpeningMonitor|HOK.Utilities$)" } |
 ForEach-Object { Remove-Item -Recurse -LiteralPath $_.FullName }
