# HOK Revit Add-ins Build System Migration Walkthrough

I have successfully migrated the HOK Revit Add-ins build system from Azure Pipelines to GitHub Actions. This document outlines the changes made, how to usage the new workflow, and the required configuration.

## 🚀 Key Changes

1.  **New Workflow File**: Created `.github/workflows/hok-revit-addins-build.yml`.
    -   **Matrix Strategy**: Automatically builds for Revit versions **2020 through 2026** in parallel.
    -   **Conditional SDK Setup**: The build toolchain is selected per Revit version:

        | Revit Version | .NET Target | Setup Action |
        | :--- | :--- | :--- |
        | 2020 | .NET Framework 4.7 | `microsoft/setup-msbuild@v2` |
        | 2021–2024 | .NET Framework 4.8 | `microsoft/setup-msbuild@v2` |
        | 2025–2026 | .NET 8 (`net8.0-windows`) | `actions/setup-dotnet@v4` |

    -   **Environment Compatibility**: Sets `BUILD_ENV` to `GitHubActions` to ensure existing scripts work correctly.
    -   **Secret Management**: Securely handles `Settings.json` and the code signing certificate.

2.  **Updated Helper Script**: Modified `_postBuild/codeSigning.ps1`.
    -   Added support for the `GitHubActions` environment.
    -   Updated logic to locate `signtool.exe` on GitHub-hosted Windows runners.

3.  **Automated Releases**:
    -   Automatically collects artifacts from all build jobs.
    -   Creates a GitHub Release on every push to `main` or `master`.

## 🛠️ How to Use

### Triggering Builds
-   **Push to `main` or `develop`**: Automatically triggers the matrix build for all Revit versions (2020-2026).
-   **Pull Requests**: Automatically triggers the matrix build for PRs targeting `main` or `develop`.
-   **Manual Trigger (Workflow Dispatch)**:
    -   Go to the **Actions** tab in GitHub.
    -   Select **HOK Revit Addins Build Workflow**.
    -   Click **Run workflow**.
    -   **Revit Version**: Leave empty to run for ALL versions, or enter a specific year (e.g., `2025`) to run a single job.
    -   **Build Configuration**: Default is `Release`. This prefix is combined with the year (e.g., `Release 25`).

### Required Secrets
You must configure the following secrets in your GitHub Repository Settings -> Secrets and variables -> Actions:

| Secret Name | Description |
| :--- | :--- |
| `SETTINGS_JSON` | **Base64 encoded** content of your `Settings.json` file. |
| `CODE_SIGNING_CERT` | **Base64 encoded** content of your `CISign.pfx` certificate. |
| `CERT_SIGNING_PASS` | Password for the code signing certificate. |

> **Note:** No private NuGet feed is required. All packages are restored from the public `nuget.org` feed.

### How to Encode Secrets
To get the Base64 string for your files, you can run this PowerShell command locally:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("path\to\your\file.json")) | Set-Clipboard
```

## 📂 Artifacts

After a successful build, the workflow produces the following artifacts (zipped):
-   `HOK-Revit-Addins-2020`
-   `HOK-Revit-Addins-2021`
-   ...
-   `HOK-Revit-Addins-2026`

Each zip file contains the fully built, signed, and organized add-in bundle ready for deployment.

## 📦 Releases

When code is pushed to the `main` branch, a **GitHub Release** is automatically created containing all the above zip files.

## ✅ Verification Checklist

- [x] Workflow YAML syntax validated.
- [x] Matrix strategy covers Revit 2020-2026.
- [x] Environment variables mapped correctly to existing PowerShell scripts.
- [x] Code signing script updated to locate `signtool.exe` on GitHub runners.
