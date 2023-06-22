$version = $args[0]
$prevVersion = "2023"

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
[System.Xml.XmlNamespaceManager]$ns = New-Object System.Xml.XmlNamespaceManager(New-Object System.Xml.NameTable)
$ns.AddNamespace('msbuild', $Namespace.msbuild)
foreach ($csprojFile in $csprojFiles) {
    Write-Host "Updating $csprojFile"
    $XPath = '//msbuild:Project/msbuild:PropertyGroup[contains(@Condition, "' + $prevVersion + '")]'
    Write-Host $XPath
    $previousVersionXml = Select-Xml $csprojFile -Namespace $Namespace -XPath $XPath
    if ($null -eq $previousVersionXml.Node) {
        Write-Host "No previous version found in $csprojFile"
        continue
    }
    if ($previousVersionXml.Node.Count -gt 1) {
        Write-Host "Multiple previous versions found in $csprojFile"
        $lastInsertedNode = $previousVersionXml.Node[$previousVersionXml.Node.Count - 1]
        foreach ($node in $previousVersionXml.Node) {
            $newVersionXml = $node.Clone()
            ## Change values in specific fields with version number in it
            if ($newVersionXml.HasAttribute('Condition')) {
                $newCondition = $newVersionXml.Condition -replace $prevVersion, $version
                $newVersionXml.SetAttribute('Condition', $newCondition)
            }
            $childrenToChange = @("OutputPath", "RevitVersion", "DefineConstants")
            foreach ($child in $childrenToChange) {
                $childNode = $newVersionXml.SelectSingleNode(".//msbuild:$child", $ns)
                $childNode.InnerText = $childNode.InnerText -replace $prevVersion, $version
            }
            $lastInsertedNode.ParentNode.InsertAfter($newVersionXml, $lastInsertedNode)
            $lastInsertedNode = $newVersionXml
        }
    } else {
        $newVersionXml = $previousVersionXml.Node.Clone()
        ## Change values in specific fields with version number in it
        if ($newVersionXml.HasAttribute('Condition')) {
            $newCondition = $newVersionXml.Condition -replace $prevVersion, $version
            $newVersionXml.SetAttribute('Condition', $newCondition)
        }
        $childrenToChange = @("OutputPath", "RevitVersion", "DefineConstants")
        foreach ($child in $childrenToChange) {
            $childNode = $newVersionXml.SelectSingleNode(".//msbuild:$child", $ns)
            $childNode.InnerText = $childNode.InnerText -replace $prevVersion, $version
        }
        $previousVersionXml.Node.ParentNode.InsertAfter($newVersionXml, $previousVersionXml.Node)
    }
    Set-Content $csprojFile $newVersionXml.OwnerDocument.OuterXml
}