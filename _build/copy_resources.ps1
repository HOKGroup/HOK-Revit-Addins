$buildConfiguration = $Env:BUILD_CONFIGURATION

$addinFolder = ".\_artifacts\{0}\" -f $buildConfiguration
$libraryFolder = "{0}\HOK-Addin.bundle\Contents\" -f $addinFolder

If ((Test-Path -PathType Container $addinFolder)) {
    Remove-Item -Path "$addinFolder\*" -Recurse
}
Else {
    New-Item -ItemType Directory -Path $addinFolder
}
New-Item -ItemType Directory -Path "$addinFolder\HOK-Addin.bundle"
New-Item -ItemType Directory -Path $libraryFolder
New-Item -ItemType Directory -Path "$libraryFolder\Resources"

Import-Csv .\_build\resources.csv | ForEach-Object { 
    $s = "{0}" -f $_.sourcePath, $buildConfiguration
    $p = Join-Path -Path $s -ChildPath $_.fileName 
    $d = "{0}{1}" -f $addinFolder, $_.destination
    $msg = "Copying {0} to {1}" -f $p, $d
    Write-Debug $msg
    Copy-Item $p -Destination $d
}

$p = ".\HOK.AddInManager\_resources\{0}\HOK{0}Addins.csv" -f $buildConfiguration
$d = "{0}\Resources" -f $libraryFolder
$msg = "Copying {0} to {1}" -f $p, $d
Write-Debug $msg
Copy-Item $p -Destination $d
