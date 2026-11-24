param([string]$Version = '1.0.1')
$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Use a numeric major.minor.patch version.' }
$root = Split-Path -Parent $PSScriptRoot
$player = Join-Path $root 'PLAY THE GAME'
foreach ($item in @('LanternGarden.exe','UnityPlayer.dll','LanternGarden_Data','MonoBleedingEdge','LanternGarden.ico')) {
    if (-not (Test-Path -LiteralPath (Join-Path $player $item))) { throw "Missing runtime item: $item" }
}
foreach ($report in @('editor-checks.txt','runtime-checks.txt')) {
    $lines = Get-Content -LiteralPath (Join-Path $root ('Docs\'+$report))
    if (-not ($lines -match '^PASS:') -or ($lines -match '^FAIL:')) { throw "Verification is incomplete: $report" }
}
if ((Get-Content -LiteralPath (Join-Path $root 'Docs\build-result.txt') -Raw) -notmatch '^Succeeded \| 0 errors \| 0 warnings') { throw 'The verified build report is missing.' }
$stage = Join-Path $root ('artifacts\Submission-v'+$Version)
$package = Join-Path $stage 'LANTERN GARDEN - COMPLETE PACKAGE'
$archive = Join-Path $root ('LanternGarden-Complete-Submission-v'+$Version+'.zip')
if ((Test-Path -LiteralPath $stage) -or (Test-Path -LiteralPath $archive)) { throw 'Submission output already exists; preserve it and use a fresh version or staging path.' }
New-Item -ItemType Directory -Path $package | Out-Null
$playCopy = Join-Path $package 'PLAY THE GAME'
New-Item -ItemType Directory -Path $playCopy | Out-Null
Get-ChildItem -LiteralPath $player -Force | Where-Object { $_.Name -notmatch 'DoNotShip|BackUpThisFolder' } | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $playCopy -Recurse
}
$projectCopy = Join-Path $package 'UnityProject'
New-Item -ItemType Directory -Path $projectCopy | Out-Null
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $root ('UnityProject\'+$folder)) -Destination $projectCopy -Recurse
}
foreach ($item in @('README.md','00-START-HERE.txt','.gitignore','Docs','Tools')) {
    Copy-Item -LiteralPath (Join-Path $root $item) -Destination $package -Recurse
}
# Compare all packaged runtime and editable project files with their originals.
foreach ($pair in @(@($player,$playCopy),@((Join-Path $root 'UnityProject\Assets'),(Join-Path $projectCopy 'Assets')),@((Join-Path $root 'UnityProject\Packages'),(Join-Path $projectCopy 'Packages')),@((Join-Path $root 'UnityProject\ProjectSettings'),(Join-Path $projectCopy 'ProjectSettings')))) {
    foreach ($file in Get-ChildItem -LiteralPath $pair[1] -File -Recurse -Force) {
        $original = Join-Path $pair[0] $file.FullName.Substring($pair[1].Length+1)
        if ((Get-FileHash -LiteralPath $original).Hash -ne (Get-FileHash -LiteralPath $file.FullName).Hash) { throw "Copy mismatch: $original" }
    }
}
$manifest = foreach ($file in Get-ChildItem -LiteralPath $package -File -Recurse -Force | Sort-Object FullName) {
    (Get-FileHash -LiteralPath $file.FullName).Hash.ToLowerInvariant()+'  '+$file.FullName.Substring($package.Length+1).Replace('\','/')
}
$manifest | Set-Content -LiteralPath (Join-Path $package 'PACKAGE-MANIFEST.sha256') -Encoding utf8
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($package,$archive,[IO.Compression.CompressionLevel]::Optimal,$true)
$zip = [IO.Compression.ZipFile]::OpenRead($archive)
try {
    $files = @(Get-ChildItem -LiteralPath $package -File -Recurse -Force)
    foreach ($file in $files) {
        $entryName = 'LANTERN GARDEN - COMPLETE PACKAGE/'+$file.FullName.Substring($package.Length+1).Replace('\','/')
        $entry = $zip.GetEntry($entryName)
        if (-not $entry) { throw "Missing ZIP entry: $entryName" }
        $stream = $entry.Open(); $sha = [Security.Cryptography.SHA256]::Create()
        try { $digest = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') }
        finally { $stream.Dispose(); $sha.Dispose() }
        if ($digest -ne (Get-FileHash -LiteralPath $file.FullName).Hash) { throw "ZIP mismatch: $entryName" }
    }
    if (@($zip.Entries | Where-Object Name).Count -ne $files.Count) { throw 'ZIP entry count mismatch.' }
} finally { $zip.Dispose() }
$hash = (Get-FileHash -LiteralPath $archive).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($archive))" | Set-Content -LiteralPath ($archive+'.sha256') -Encoding ASCII
[pscustomobject]@{Archive=$archive;Files=$files.Count;Bytes=(Get-Item -LiteralPath $archive).Length;SHA256=$hash} | ConvertTo-Json
