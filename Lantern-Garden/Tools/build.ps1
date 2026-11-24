param([string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
New-Item -ItemType Directory -Path (Join-Path $root 'artifacts') -Force | Out-Null
$projectPath = Join-Path $root 'UnityProject'
$logPath = Join-Path $root 'artifacts\build.log'
$buildProcess = Start-Process -FilePath $UnityPath -ArgumentList @('-batchmode','-quit','-force-d3d11','-projectPath',('"'+$projectPath+'"'),'-executeMethod','LanternGarden.Editor.GardenBuilder.CreateAndBuild','-logFile',('"'+$logPath+'"')) -WindowStyle Hidden -Wait -PassThru
if ($buildProcess.ExitCode -ne 0) { throw 'Unity build failed. See artifacts\build.log.' }
Get-Content -LiteralPath (Join-Path $root 'Docs\build-result.txt')
