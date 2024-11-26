$buildConfiguration = $Env:BUILD_CONFIGURATION

$addinFolder = Join-Path .\_artifacts\ $buildConfiguration

Import-Csv .\_build\files.csv | ForEach-Object { 
    $s = "{0}{1}" -f $_.sourcePath,$buildConfiguration
    $p = Join-Path -Path $s -ChildPath $_.fileName 
    $d = "{0}\{1}" -f $addinFolder,$_.destination
    If ((Test-Path $p)) {
        Copy-Item $p -Destination $d
    }
}
