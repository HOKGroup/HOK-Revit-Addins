$buildConfiguration = $Env:BUILD_CONFIGURATION

$addinFolder = Join-Path .\_artifacts\ $buildConfiguration
$libraryFolder = Join-Path $addinFolder \HOK-Addin.bundle\Contents\

$list = Import-Csv .\_build\files.csv | ForEach { 
    $s = "{0}{1}" -f $_.sourcePath,$buildConfiguration
    $p = Join-Path -Path $s -ChildPath $_.fileName 
    $d = "{0}\{1}" -f $addinFolder,$_.destination
    Copy-Item $p -Destination $d
}
