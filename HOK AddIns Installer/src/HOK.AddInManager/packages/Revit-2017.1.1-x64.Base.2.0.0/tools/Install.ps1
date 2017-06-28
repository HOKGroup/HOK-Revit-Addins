<#
Install.ps1
Revit-2017x64 NuGet package.

© Andrey Bushman, 2016
https://www.nuget.org/profiles/Bush

This PowerShell script will be launched by NuGet each time when
this package will be installed into the Visual Studio project.

This script sets `CopyLocal` to `false` for each Revit assembly.
#>
param($installPath, $toolsPath, $package, $project)

$asm_root_folder_name = [System.IO.Path]::Combine($installPath,`
"lib");

$net_folders = [System.IO.Directory]::GetDirectories(`
$asm_root_folder_name, 'net*', 'TopDirectoryOnly');

$file_names = New-Object `
'System.Collections.Generic.HashSet[string]';

foreach ($net in $net_folders) {

    $files = [System.IO.Directory]::EnumerateFiles($net,"*.dll"`
    ,"TopDirectoryOnly");

        foreach ($file in $files) {

            $file_name = [System.IO.Path]::`
            GetFileNameWithoutExtension($file);

            $file_names.Add($file_name);
    }
}

foreach ($reference in $project.Object.References) {

    if($file_names.Contains($reference.Name)) {

        $reference.CopyLocal = $false;
    }
}