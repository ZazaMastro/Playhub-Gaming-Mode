param(
  [string]$ConfigPath = "C:\Program Files\Sunshine\config\sunshine.conf",
  [string]$MarkerPath = "$env:TEMP\gaming-mode-sunshine-surround-fix.txt"
)

$ErrorActionPreference = "Stop"

function Set-ConfigValue {
  param(
    [System.Collections.Generic.List[string]]$Lines,
    [string]$Key,
    [string]$Value
  )

  $found = $false
  for ($i = 0; $i -lt $Lines.Count; $i++) {
    if ($Lines[$i].Trim() -match "^$([regex]::Escape($Key))\s*=") {
      if (-not $found) {
        $Lines[$i] = "$Key = $Value"
        $found = $true
      }
      else {
        $Lines.RemoveAt($i)
        $i--
      }
    }
  }

  if (-not $found) {
    $Lines.Add("$Key = $Value")
  }
}

try {
  if (-not (Test-Path -LiteralPath $ConfigPath)) {
    throw "Sunshine config not found: $ConfigPath"
  }

  $backupPath = "$ConfigPath.bak-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
  Copy-Item -LiteralPath $ConfigPath -Destination $backupPath -Force

  $lines = [System.Collections.Generic.List[string]]::new()
  foreach ($line in Get-Content -LiteralPath $ConfigPath) {
    $lines.Add($line)
  }

  Set-ConfigValue -Lines $lines -Key "stream_audio" -Value "enabled"
  Set-ConfigValue -Lines $lines -Key "install_steam_audio_drivers" -Value "enabled"
  Set-ConfigValue -Lines $lines -Key "virtual_sink" -Value "Steam Streaming Speakers"
  Set-ConfigValue -Lines $lines -Key "audio_sink" -Value ""

  Set-Content -LiteralPath $ConfigPath -Value $lines -Encoding UTF8

  $service = Get-Service -Name "SunshineService" -ErrorAction SilentlyContinue
  if ($service) {
    Restart-Service -Name "SunshineService" -Force
  }
  else {
    Get-Process -Name "sunshine" -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Process -FilePath "C:\Program Files\Sunshine\sunshine.exe" -WorkingDirectory "C:\Program Files\Sunshine"
  }

  $audioInfo = ""
  $audioInfoPath = "C:\Program Files\Sunshine\tools\audio-info.exe"
  if (Test-Path -LiteralPath $audioInfoPath) {
    Start-Sleep -Seconds 2
    $audioInfo = & $audioInfoPath | Out-String
  }

  @(
    "OK"
    "Config: $ConfigPath"
    "Backup: $backupPath"
    ""
    "Sunshine audio settings:"
    (Get-Content -LiteralPath $ConfigPath | Select-String -Pattern "^(audio_sink|virtual_sink|stream_audio|install_steam_audio_drivers)\s*=" | Out-String)
    ""
    "Audio devices:"
    $audioInfo
  ) | Set-Content -LiteralPath $MarkerPath -Encoding UTF8
}
catch {
  @(
    "ERROR"
    $_.Exception.Message
  ) | Set-Content -LiteralPath $MarkerPath -Encoding UTF8
  throw
}
