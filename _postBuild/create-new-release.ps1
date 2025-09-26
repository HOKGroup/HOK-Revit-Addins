$latestBuildDir = 'master'
## Create release number folder (from parent folder name)
$releaseDir = $(Split-Path -Path $(Get-Location) -Leaf)
mkdir $releaseDir


## Copy from prev to release number
$yearDirCount = $(Get-ChildItem -Path .\prev\ -Directory |  Where-Object { $_.Name -match '^\d{4}' } | Measure-Object).Count
if ($yearDirCount -eq 0) {
	Write-Error 'Directory is nested, no year folders found'
	return 1
}
Get-ChildItem -Path .\prev\ -Directory | Where-Object { $_.Name -match '^\d{4}' } | ForEach-Object { Copy-Item -Path $_.FullName -Destination $releaseDir -Force -Recurse }


## Copy from master to release number folder
$yearDirCount = $(Get-ChildItem -Path $latestBuildDir -Directory |  Where-Object { $_.Name -match '^\d{4}' } | Measure-Object).Count
if ($yearDirCount -eq 0) {
	$yearDirCount = $(Get-ChildItem -Path .\$latestBuildDir\*\* -Directory |  Where-Object { $_.Name -match '^\d{4}' } | Measure-Object).Count
	if ($yearDirCount -eq 0) {
		Write-Error 'Directory is nested, no year folders found'
		return 1
	}
 else {
		$latestBuildDir = ".\$latestBuildDir\*\*"
	}
}
Get-ChildItem -Path $latestBuildDir -Directory | Where-Object { $_.Name -match '^\d{4}' } | ForEach-Object { Copy-Item -Path $_.FullName -Destination $releaseDir -Force -Recurse }
Write-Host 'Success!'
return 0

## TODO: Add step to zip them
