param([ValidateSet('verify','windows','web','open')][string]$Action = 'verify')
$ErrorActionPreference = 'Stop'
$artifactPath = Join-Path $PSScriptRoot '..\artifacts'
New-Item -ItemType Directory -Path $artifactPath -Force | Out-Null
Set-Content -LiteralPath (Join-Path $artifactPath 'editor-request.txt') -Value $Action
Write-Output "Requested $Action. Open the Neon Relay Unity project; results will appear in artifacts."
