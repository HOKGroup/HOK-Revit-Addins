Write-Output "Signing DLLs"

$DLL_PATH = $args[0]

If ($Env:BUILD_ENV -eq 'AzureDevOps') {
    Write-Output "PFX_PATH: $Env:PFX_PATH"
    Write-Output "PFX_PASS: $Env:PFX_PASS"
    $s = '"C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /f $Env:PFX_PATH /p "$Env:PFX_PASS" /t http://timestamp.comodoca.com/authenticode $DLL_PATH'
    Write-Output "COMMAND: $s"
    &"C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /f $Env:PFX_PATH /p "$Env:PFX_PASS" /t http://timestamp.comodoca.com/authenticode $DLL_PATH
} Else {
    &"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\signtool.exe" sign /c '2021 Code Signing - DTM ' /t http://timestamp.comodoca.com/authenticode $DLL_PATH
}

