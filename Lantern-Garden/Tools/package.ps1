param([string]$Version = '1.0.1')
$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Use a numeric major.minor.patch version.' }
$root = Split-Path -Parent $PSScriptRoot
$player = Join-Path $root 'PLAY THE GAME'
foreach ($required in @('LanternGarden.exe','UnityPlayer.dll','LanternGarden_Data','MonoBleedingEdge')) {
    if (-not (Test-Path -LiteralPath (Join-Path $player $required))) { throw "Build the game first. Missing: $required" }
}
foreach ($report in @('editor-checks.txt','runtime-checks.txt','VERIFICATION.md')) {
    if (-not (Test-Path -LiteralPath (Join-Path $root ('Docs\'+$report)))) { throw "Complete verification first. Missing: $report" }
}
if (Select-String -LiteralPath (Join-Path $root 'Docs\runtime-checks.txt') -Pattern '^FAIL:') { throw 'Runtime checks contain a failure.' }
$stage = Join-Path $root ('artifacts\Packages-v'+$Version)
if (Test-Path -LiteralPath $stage) { throw 'Packaging staging already exists. Preserve it and use a new location for a new release.' }
New-Item -ItemType Directory -Path $stage | Out-Null
$playCopy = Join-Path $stage ('LanternGarden-Windows-v'+$Version)
$sourceCopy = Join-Path $stage ('LanternGarden-Unity-Source-v'+$Version)
New-Item -ItemType Directory -Path $playCopy,$sourceCopy | Out-Null
Get-ChildItem -LiteralPath $player | Where-Object { $_.Name -notmatch 'DoNotShip|BackUpThisFolder' } | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $playCopy -Recurse
}
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
    $destination = Join-Path $sourceCopy 'UnityProject'
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $root ('UnityProject\'+$folder)) -Destination $destination -Recurse
}
foreach ($item in @('README.md','.gitignore','Docs','Tools')) { Copy-Item -LiteralPath (Join-Path $root $item) -Destination $sourceCopy -Recurse }
Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($folder in @($playCopy,$sourceCopy)) {
    $zipPath = Join-Path $root ((Split-Path -Leaf $folder)+'.zip')
    if (Test-Path -LiteralPath $zipPath) { throw "Archive already exists: $zipPath" }
    [IO.Compression.ZipFile]::CreateFromDirectory($folder,$zipPath,[IO.Compression.CompressionLevel]::Optimal,$true)
    $zip = [IO.Compression.ZipFile]::OpenRead($zipPath)
    try {
        $files = @(Get-ChildItem -LiteralPath $folder -Recurse -File -Force)
        foreach ($file in $files) {
            $entryPath = (Split-Path -Leaf $folder)+'/'+$file.FullName.Substring($folder.Length+1).Replace('\','/')
            $entry = $zip.GetEntry($entryPath)
            if (-not $entry) { throw "Missing ZIP entry: $entryPath" }
            $stream = $entry.Open(); $sha = [Security.Cryptography.SHA256]::Create()
            try { $hash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') }
            finally { $stream.Dispose(); $sha.Dispose() }
            if ($hash -ne (Get-FileHash -LiteralPath $file.FullName).Hash) { throw "ZIP mismatch: $entryPath" }
        }
        if (@($zip.Entries | Where-Object Name).Count -ne $files.Count) { throw 'ZIP file count mismatch.' }
    } finally { $zip.Dispose() }
    $digest = (Get-FileHash -LiteralPath $zipPath).Hash.ToLowerInvariant()
    "$digest  $([IO.Path]::GetFileName($zipPath))" | Set-Content -LiteralPath ($zipPath+'.sha256') -Encoding ASCII
    [PSCustomObject]@{Archive=$zipPath;Files=$files.Count;Bytes=(Get-Item -LiteralPath $zipPath).Length;SHA256=$digest}
}
