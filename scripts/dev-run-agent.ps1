$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
dotnet run --project (Join-Path $RepoRoot "src\GamingMode\GamingMode.csproj") -- agent

