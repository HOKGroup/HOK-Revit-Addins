## Add new version to .sln files
##$version = $args[0]

## Find all .sln files
$slnFiles = Get-ChildItem -Path . -Filter *.sln -Recurse

$SlnSearchExp = '(?<DebugLine>(?<Indent>\s+)Debug\|x64 = Debug\|x64)'
$SlnReplaceExp = '${Indent}2024|x64 = 2024|x64
${DebugLine}'

$ProjectSearchExp = '(?<DebugLine>(?<Indent>\s+)\{((?<ProjectId>\w{8}-\w{4}-\w{4}-\w{4}-\w{12})\})\.Debug\|x64\.(ActiveCfg)(\.0)? = Debug\|x64)'
$ProjectReplaceExp = '${Indent}{${ProjectId}}.2024|x64.ActiveCfg = 2024|x64
${Indent}{${ProjectId}}.2024|x64.Build.0 = 2024|x64
${Indent}{${ProjectId}}.Debug|x64.ActiveCfg = Debug|x64'

foreach ($slnFile in $slnFiles) {
    (Get-Content $slnFile) | ForEach-Object { 
        $_ -replace $SlnSearchExp, $SlnReplaceExp 
    } | Set-Content $slnFile
    (Get-Content $slnFile) | ForEach-Object { 
        $_ -replace $ProjectSearchExp, $ProjectReplaceExp 
    } | Set-Content $slnFile
}