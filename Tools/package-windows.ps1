param([ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '1.1.0')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$buildDir = Join-Path $repoRoot 'Neon Relay\Builds\Windows'
$assembly = Join-Path $buildDir 'NeonRelay_Data\Managed\Assembly-CSharp.dll'
foreach ($required in @('NeonRelay.exe', 'UnityPlayer.dll', 'MonoBleedingEdge', 'NeonRelay_Data\Managed\Assembly-CSharp.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $buildDir $required))) { throw "Incomplete build: $required" }
}
$artifactDir = Join-Path $repoRoot 'artifacts'
$packageDir = Join-Path $artifactDir "NeonRelay-Windows-v$Version"
$archive = "$packageDir.zip"
if ((Test-Path -LiteralPath $packageDir) -or (Test-Path -LiteralPath $archive)) {
    throw 'This version is already packaged. Use a new version or explicitly remove the previous package after review.'
}
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
Get-ChildItem -LiteralPath $buildDir | Where-Object { $_.Name -notlike '*DoNotShip*' } | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $packageDir -Recurse
}
foreach ($document in @('README.md', 'CHANGELOG.md', 'Docs')) {
    Copy-Item -LiteralPath (Join-Path $repoRoot $document) -Destination $packageDir -Recurse
}
@"
NEON RELAY $Version - WINDOWS x64

Extract this WHOLE folder, then run NeonRelay.exe.
Keep NeonRelay_Data, MonoBleedingEdge, and the DLL files beside the executable.
Unity is not required to play.

A/D or arrows: move. Space/W/Up: jump; press again in the air.
E: connect to relay. 1/2/3: toggle lines. Enter: send power / continue.
Esc: pause. R: reset sector. H: field manual/options. F11: fullscreen. M: sound.

Recover 3 cells, solve the relay, and reach the exit in each sector.
Checkpoint retries keep cells and solved relays. There is no lives limit.

Docs/SUBMISSION.md contains the game design and demonstration guide.
Docs/VERIFICATION.md records tested features and platform scope.
Source project and releases: https://github.com/ogunmefun/2d-project
"@ | Set-Content -LiteralPath (Join-Path $packageDir 'PLAYME.txt') -Encoding UTF8
Compress-Archive -LiteralPath $packageDir -DestinationPath $archive -CompressionLevel Optimal
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($archive)
try {
    if ($zip.Entries | Where-Object { $_.FullName -like '*DoNotShip*' }) { throw 'Debug files leaked into the archive.' }
    $entry = $zip.Entries | Where-Object { $_.FullName.Replace('\','/').EndsWith('/NeonRelay_Data/Managed/Assembly-CSharp.dll') }
    if (-not $entry) { throw 'Game assembly is missing from the archive.' }
    $stream = $entry.Open()
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $packedHash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') }
    finally { $stream.Dispose(); $sha.Dispose() }
    if ($packedHash -ne (Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash) { throw 'Packed game assembly differs from the build.' }
    $count = $zip.Entries.Count
} finally { $zip.Dispose() }
$archiveHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
"$archiveHash  $([IO.Path]::GetFileName($archive))" | Set-Content -LiteralPath "$archive.sha256" -Encoding ASCII
[PSCustomObject]@{ Archive = $archive; Bytes = (Get-Item -LiteralPath $archive).Length; Entries = $count; SHA256 = $archiveHash }
