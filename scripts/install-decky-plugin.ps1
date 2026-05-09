param(
  [string]$DeckyPluginsDir = "",
  [string]$BuiltPluginDir = ""
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($BuiltPluginDir)) {
  $BuiltPluginDir = Join-Path $RepoRoot "artifacts\decky-plugin\gaming-mode"
}

if ([string]::IsNullOrWhiteSpace($DeckyPluginsDir)) {
  $Candidates = @(
    (Join-Path $env:LOCALAPPDATA "decky-loader\plugins"),
    (Join-Path $env:LOCALAPPDATA "Programs\decky-loader\plugins"),
    "C:\homebrew\plugins"
  )

  $DeckyPluginsDir = $Candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($DeckyPluginsDir)) {
  throw "Decky plugin directory was not found. Pass -DeckyPluginsDir."
}

if (-not (Test-Path $BuiltPluginDir)) {
  throw "Built plugin directory was not found at $BuiltPluginDir. Run scripts\build.ps1 first."
}

$Destination = Join-Path $DeckyPluginsDir "gaming-mode"
if (Test-Path $Destination) {
  Remove-Item -LiteralPath $Destination -Recurse -Force
}

Copy-Item -Path $BuiltPluginDir -Destination $Destination -Recurse
Write-Host "Installed Decky plugin to $Destination"

