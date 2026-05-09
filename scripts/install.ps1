param(
  [string]$SourceDir = "",
  [switch]$NoStartupTask
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($SourceDir)) {
  if (Test-Path (Join-Path $PSScriptRoot "GamingMode.exe")) {
    $SourceDir = $PSScriptRoot
  }
  elseif (Test-Path (Join-Path $RepoRoot "artifacts\release\gaming-mode\GamingMode.exe")) {
    $SourceDir = Join-Path $RepoRoot "artifacts\release\gaming-mode"
  }
  else {
    $SourceDir = Join-Path $RepoRoot "artifacts\app"
  }
}

$SourceDir = (Resolve-Path $SourceDir).Path
$ExeSource = Join-Path $SourceDir "GamingMode.exe"
if (-not (Test-Path $ExeSource)) {
  throw "GamingMode.exe was not found in $SourceDir. Run scripts\build.ps1 first or pass -SourceDir."
}

$InstallDir = Join-Path $env:LOCALAPPDATA "GamingMode"
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null

Get-Process -Name "GamingMode" -ErrorAction SilentlyContinue | ForEach-Object {
  try {
    if (-not $_.CloseMainWindow()) {
      $_.Kill()
      return
    }

    if (-not $_.WaitForExit(3000)) {
      $_.Kill()
    }
  }
  catch {
  }
}

Copy-Item -Path (Join-Path $SourceDir "*") -Destination $InstallDir -Recurse -Force

$Exe = Join-Path $InstallDir "GamingMode.exe"
$Icon = Join-Path $InstallDir "assets\logo.ico"
$DesktopShortcut = Join-Path ([Environment]::GetFolderPath("Desktop")) "Gaming Mode.lnk"
$StartMenuDir = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\Gaming Mode"
$StartMenuShortcut = Join-Path $StartMenuDir "Gaming Mode.lnk"
New-Item -ItemType Directory -Force -Path $StartMenuDir | Out-Null

$Shell = New-Object -ComObject WScript.Shell
foreach ($ShortcutPath in @($DesktopShortcut, $StartMenuShortcut)) {
  $Shortcut = $Shell.CreateShortcut($ShortcutPath)
  $Shortcut.TargetPath = $Exe
  $Shortcut.WorkingDirectory = $InstallDir
  $Shortcut.Description = "Switch between Desktop Mode and Steam Gaming Mode"
  if (Test-Path $Icon) {
    $Shortcut.IconLocation = $Icon
  }
  $Shortcut.Save()
}

if (-not $NoStartupTask) {
  $StartupShortcut = Join-Path ([Environment]::GetFolderPath("Startup")) "Gaming Mode Agent.lnk"
  $Startup = $Shell.CreateShortcut($StartupShortcut)
  $Startup.TargetPath = $Exe
  $Startup.Arguments = "agent --boot"
  $Startup.WorkingDirectory = $InstallDir
  $Startup.Description = "Start Gaming Mode Agent"
  $Startup.WindowStyle = 7
  if (Test-Path $Icon) {
    $Startup.IconLocation = $Icon
  }
  $Startup.Save()
}

Start-Process -FilePath $Exe -ArgumentList "agent" -WindowStyle Hidden -WorkingDirectory $InstallDir
$AgentReady = $false
for ($i = 0; $i -lt 20; $i++) {
  Start-Sleep -Milliseconds 250
  try {
    $Health = Invoke-WebRequest -Uri "http://127.0.0.1:47991/health" -UseBasicParsing -TimeoutSec 1
    if ($Health.StatusCode -eq 200) {
      $AgentReady = $true
      break
    }
  }
  catch {
  }
}

Start-Process -FilePath $Exe -WorkingDirectory $InstallDir
Write-Host "Installed Gaming Mode to $InstallDir"
Write-Host "Desktop shortcut: $DesktopShortcut"
Write-Host "Start Menu shortcut: $StartMenuShortcut"
if ($AgentReady) {
  Write-Host "Local agent is running on http://127.0.0.1:47991"
}
else {
  Write-Host "WARNING: Local agent did not answer on http://127.0.0.1:47991"
}
