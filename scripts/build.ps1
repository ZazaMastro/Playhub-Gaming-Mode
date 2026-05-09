param(
  [string]$Configuration = "Release",
  [switch]$SkipDecky
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true
$RepoRoot = Split-Path -Parent $PSScriptRoot
$Artifacts = Join-Path $RepoRoot "artifacts"
$PublishRoot = Join-Path $Artifacts "app"
$SetupPublishRoot = Join-Path $Artifacts "setup"
$ReleaseRoot = Join-Path $Artifacts "release"
$WindowsPackageRoot = Join-Path $ReleaseRoot "gaming-mode"
$WindowsExpandedRoot = Join-Path $Artifacts "gaming-mode-win-x64"
$PluginRoot = Join-Path $RepoRoot "decky-plugin"
$PluginPackageRoot = Join-Path $Artifacts "decky-plugin"
$PluginPackageDir = Join-Path $PluginPackageRoot "gaming-mode"
$AssetsRoot = Join-Path $RepoRoot "assets"
$ReleaseAssets = @("base-logo.png", "logo.ico")

New-Item -ItemType Directory -Force -Path $Artifacts | Out-Null

$ArchiveRoot = Join-Path $Artifacts "archive"
$ArchiveStamp = Get-Date -Format "yyyyMMdd-HHmmss"
$ArchivedZips = @()
New-Item -ItemType Directory -Force -Path $ArchiveRoot | Out-Null
foreach ($ExistingZipName in @("gaming-mode-win-x64.zip", "gaming-mode-decky.zip")) {
  $ExistingZip = Join-Path $Artifacts $ExistingZipName
  if (Test-Path $ExistingZip) {
    $ArchivedZipName = [System.IO.Path]::GetFileNameWithoutExtension($ExistingZipName) + "-$ArchiveStamp.zip"
    $ArchivedZip = Join-Path $ArchiveRoot $ArchivedZipName
    Copy-Item -LiteralPath $ExistingZip -Destination $ArchivedZip -Force
    $ArchivedZips += $ArchivedZip
  }
}

$ExpandedPackageAvailable = $true

foreach ($PathToClean in @($PublishRoot, $SetupPublishRoot, $ReleaseRoot, $PluginPackageRoot)) {
  if (Test-Path $PathToClean) {
    Remove-Item -LiteralPath $PathToClean -Recurse -Force
  }
}

if (Test-Path $WindowsExpandedRoot) {
  try {
    Remove-Item -LiteralPath $WindowsExpandedRoot -Recurse -Force
  }
  catch {
    $ExpandedPackageAvailable = $false
    Write-Warning "Expanded package folder is in use and will not be refreshed: $WindowsExpandedRoot"
  }
}

dotnet publish (Join-Path $RepoRoot "src\GamingMode\GamingMode.csproj") `
  --configuration $Configuration `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o $PublishRoot

if ($LASTEXITCODE -ne 0) {
  throw "dotnet publish failed with exit code $LASTEXITCODE"
}

dotnet publish (Join-Path $RepoRoot "src\GamingModeSetup\GamingModeSetup.csproj") `
  --configuration $Configuration `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o $SetupPublishRoot

if ($LASTEXITCODE -ne 0) {
  throw "setup publish failed with exit code $LASTEXITCODE"
}

if (Test-Path $AssetsRoot) {
  foreach ($TargetRoot in @($PublishRoot, $SetupPublishRoot)) {
    $TargetAssets = Join-Path $TargetRoot "assets"
    New-Item -ItemType Directory -Force -Path $TargetAssets | Out-Null
    foreach ($AssetName in $ReleaseAssets) {
      $AssetPath = Join-Path $AssetsRoot $AssetName
      if (Test-Path $AssetPath) {
        Copy-Item -LiteralPath $AssetPath -Destination $TargetAssets -Force
      }
    }
  }
}

if (Test-Path $WindowsPackageRoot) {
  Remove-Item -LiteralPath $WindowsPackageRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $WindowsPackageRoot | Out-Null
Get-ChildItem -Path $PublishRoot -Force | Where-Object { $_.Extension -ne ".pdb" } | ForEach-Object {
  Copy-Item -LiteralPath $_.FullName -Destination $WindowsPackageRoot -Recurse -Force
}
Copy-Item -Path (Join-Path $SetupPublishRoot "GamingModeSetup.exe") -Destination (Join-Path $WindowsPackageRoot "Setup.exe")
Copy-Item -Path (Join-Path $RepoRoot "scripts\install.ps1") -Destination $WindowsPackageRoot
Copy-Item -Path (Join-Path $RepoRoot "scripts\uninstall.ps1") -Destination $WindowsPackageRoot
Copy-Item -Path (Join-Path $RepoRoot "Install Gaming Mode.cmd") -Destination $WindowsPackageRoot
Copy-Item -Path (Join-Path $RepoRoot "Uninstall Gaming Mode.cmd") -Destination $WindowsPackageRoot
Copy-Item -Path (Join-Path $RepoRoot "README.md") -Destination $WindowsPackageRoot
Copy-Item -Path (Join-Path $RepoRoot "LICENSE") -Destination $WindowsPackageRoot

if (-not $SkipDecky) {
  Push-Location $PluginRoot
  try {
    if (-not (Test-Path "node_modules")) {
      npm install
      if ($LASTEXITCODE -ne 0) {
        throw "npm install failed with exit code $LASTEXITCODE"
      }
    }
    npm run build
    if ($LASTEXITCODE -ne 0) {
      throw "npm run build failed with exit code $LASTEXITCODE"
    }
  }
  finally {
    Pop-Location
  }

  if (Test-Path $PluginPackageRoot) {
    Remove-Item -LiteralPath $PluginPackageRoot -Recurse -Force
  }

  New-Item -ItemType Directory -Force -Path $PluginPackageDir | Out-Null
  Copy-Item -Path (Join-Path $PluginRoot "plugin.json") -Destination $PluginPackageDir
  Copy-Item -Path (Join-Path $PluginRoot "package.json") -Destination $PluginPackageDir
  Copy-Item -Path (Join-Path $PluginRoot "README.md") -Destination $PluginPackageDir -ErrorAction SilentlyContinue
  $PluginDistDir = Join-Path $PluginPackageDir "dist"
  New-Item -ItemType Directory -Force -Path $PluginDistDir | Out-Null
  Get-ChildItem -Path (Join-Path $PluginRoot "dist") -File | Where-Object { $_.Extension -ne ".map" } | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $PluginDistDir -Force
  }

  $ZipPath = Join-Path $Artifacts "gaming-mode-decky.zip"
  if (Test-Path $ZipPath) {
    Remove-Item -LiteralPath $ZipPath -Force
  }
  Compress-Archive -Path $PluginPackageDir -DestinationPath $ZipPath
}

if ($ExpandedPackageAvailable) {
  New-Item -ItemType Directory -Force -Path $WindowsExpandedRoot | Out-Null
  Get-ChildItem -Path $WindowsPackageRoot -Force | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $WindowsExpandedRoot -Recurse -Force
  }
}

$WindowsZipPath = Join-Path $Artifacts "gaming-mode-win-x64.zip"
if (Test-Path $WindowsZipPath) {
  Remove-Item -LiteralPath $WindowsZipPath -Force
}
Compress-Archive -Path (Join-Path $WindowsPackageRoot "*") -DestinationPath $WindowsZipPath

Write-Host "Companion app: $PublishRoot"
Write-Host "Setup app: $SetupPublishRoot"
Write-Host "Release zip: $WindowsZipPath"
if (-not $SkipDecky) {
  Write-Host "Decky plugin zip: $(Join-Path $Artifacts "gaming-mode-decky.zip")"
}
foreach ($ArchivedZip in $ArchivedZips) {
  Write-Host "Archived previous zip: $ArchivedZip"
}
