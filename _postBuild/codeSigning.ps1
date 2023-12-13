Write-Output "Signing DLLs"

$DLL_PATH = $args[0]

If ($Env:BUILD_ENV -eq 'AzureDevOps') {
    $p = $Env:PFX_PASS
    &"C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /f $Env:PFX_PATH /p $p /t http://timestamp.comodoca.com/authenticode $DLL_PATH
} Else {
    &"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\signtool.exe" sign /c 'Code Signing - DTM HOK-CA' /t http://timestamp.comodoca.com/authenticode $DLL_PATH
}

