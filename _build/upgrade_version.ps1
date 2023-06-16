$version = $args[0]

#================= Add new version to .sln files ===================
## Find all .sln files
$slnFiles = Get-ChildItem -Path . -Filter *.sln -Recurse

$SlnSearchExp = '(?<DebugLine>(?<Indent>\s+)Debug\|x64 = Debug\|x64)'
$SlnReplaceExp = '${Indent}' + $([regex]::escape($version)) + '|x64 = ' + $([regex]::escape($version)) + '|x64
${DebugLine}'

$ProjectSearchExp = '(?<DebugLine>(?<Indent>\s+)\{((?<ProjectId>\w{8}-\w{4}-\w{4}-\w{4}-\w{12})\})\.Debug\|x64\.(ActiveCfg)(\.0)? = Debug\|x64)'
$ProjectReplaceExp = '${Indent}{${ProjectId}}.' + $([regex]::escape($version)) + '|x64.ActiveCfg = ' + $([regex]::escape($version)) + '|x64
${Indent}{${ProjectId}}.' + $([regex]::escape($version)) + '|x64.Build.0 = ' + $([regex]::escape($version)) + '|x64
${Indent}{${ProjectId}}.Debug|x64.ActiveCfg = Debug|x64'

foreach ($slnFile in $slnFiles) {
    (Get-Content $slnFile) | ForEach-Object { 
        $_ -replace $SlnSearchExp, $SlnReplaceExp 
    } | Set-Content $slnFile
    (Get-Content $slnFile) | ForEach-Object { 
        $_ -replace $ProjectSearchExp, $ProjectReplaceExp 
    } | Set-Content $slnFile
}

#================= Add new version to .csproj files ===================
$csprojFiles = Get-ChildItem -Path . -Filter *.csproj -Recurse
$Namespace = @{msbuild ='http://schemas.microsoft.com/developer/msbuild/2003'}
foreach ($csprojFile in $csprojFiles[0..1]) {
    [xml]$xml = Get-Content $csprojFile
    $c = Select-Xml $csprojFile -Namespace $Namespace -XPath '//msbuild:Project/msbuild:PropertyGroup'

    foreach ($node in $c) {
        $v = Select-Xml -Xml $node.node -XPath '//RevitVersion'
        $v.Node.InnerText
    }
    # $c | Format-Table @{Label="Name"; Expression= {($_.node.innerxml).trim()}}, Path -AutoSize
    # $a = Select-Xml -Xml $xml

}