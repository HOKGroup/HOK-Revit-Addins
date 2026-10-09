param([string]$ReleaseTag="0.1.0.28")
$response = Invoke-RestMethod -Uri "https://api.github.com/repos/HOKGroup/HOK-Revit-Addins/releases"
$current_release = $response | Where-Object { $_.tag_name -eq $ReleaseTag }

$zombieJsonTemplate = [ordered]@{
    "ShouldSerialize"   = $false
    "Address"           = "https://github.com/HOKGroup/HOK-Revit-Addins"
    "Frequency"         = "h2"
    "SourceAssets"      = @()
    "DestinationAssets" = @( $current_release.assets | ForEach-Object {
            [ordered]@{
                "LocationType"  = "Folder"
                "DirectoryPath" = "C:\ProgramData\Autodesk\Revit\Addins\$($_.name.Replace('.zip', ''))"
                "Assets"        = @(
                    [ordered]@{
                        "id"   = $_.id
                        "name" = $_.name
                        "url"  = $_.url
                    }
                )
            }
        }
    )
    "LatestRelease"     = [ordered]@{
        "id"           = $current_release.id
        "tag_name"     = $current_release.tag_name
        "name"         = $current_release.name
        "body"         = $current_release.body
        "prerelease"   = $current_release.prerelease
        "published_at" = $current_release.published_at
        "author"       = @{
            "login" = $current_release.author.login
        }
        "assets"       = @( $current_release.assets | ForEach-Object {
                [ordered]@{
                    "name" = $_.name
                    "url"  = $_.url
                    "id"   = $_.id
                }
            }
        )
    }
}

$zombieJsonTemplate | ConvertTo-Json -Depth 5
