param(
    [string]$OutputDirectory,
    [string]$ArchivePath,
    [switch]$AssembleOnly,
    [switch]$ArchiveExisting
)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $OutputDirectory) { $OutputDirectory = Join-Path (Split-Path -Parent $repoRoot) 'NEON RELAY - COMPLETE PACKAGE' }
if (-not $ArchivePath) { $ArchivePath = Join-Path (Split-Path -Parent $repoRoot) 'NeonRelay-Complete-Package-v1.2.0.zip' }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$ArchivePath = [IO.Path]::GetFullPath($ArchivePath)
if ($ArchivePath.StartsWith($OutputDirectory + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) {
    throw 'The ZIP must be outside the folder being archived.'
}
$noteName = '00-FOLDER-CONTENTS.txt'
$buildDir = Join-Path $repoRoot 'Neon Relay\Builds\Windows'
$sourceDir = Join-Path $OutputDirectory '02-Source-and-Build'
$guideDir = Join-Path $OutputDirectory '01-Game-Guide'
$checksDir = Join-Path $OutputDirectory '03-Verification'

foreach ($required in @('NeonRelay.exe','UnityPlayer.dll','MonoBleedingEdge','NeonRelay_Data\Managed\Assembly-CSharp.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $buildDir $required))) { throw "Build the Windows player first. Missing: $required" }
}
if ($OutputDirectory.StartsWith($repoRoot + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) {
    # artifacts is ignored by Git; an output in another source folder would recursively package itself.
    if (-not $OutputDirectory.StartsWith((Join-Path $repoRoot 'artifacts') + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) {
        throw 'Choose an output beside the repository, or under its ignored artifacts folder.'
    }
}
if (Test-Path -LiteralPath $ArchivePath) { throw 'Archive already exists. Choose a new archive path; existing packages are preserved.' }
if ($ArchiveExisting) {
    if (-not (Test-Path -LiteralPath (Join-Path $OutputDirectory 'NeonRelay.exe'))) { throw 'Assemble the folder before archiving it.' }
} else {
    if (Test-Path -LiteralPath $OutputDirectory) { throw 'Output already exists. Use -ArchiveExisting after reviewing it, or choose a new output folder.' }
    New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
    Get-ChildItem -LiteralPath $buildDir | Where-Object { $_.Name -notlike '*DoNotShip*' } | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $OutputDirectory -Recurse
    }
}
foreach ($directory in @($sourceDir,$guideDir,$checksDir,(Join-Path $guideDir 'Screenshots'))) {
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
}

# Preserve the complete source repository layout so README links and build scripts still work.
# Do not include .git, editor caches, local logs, previous builds, or local credentials.
$revision = 'Source export without Git metadata'
if (Test-Path -LiteralPath (Join-Path $repoRoot '.git')) {
    $sourceFiles = @(git -C $repoRoot ls-files --cached)
    if ($LASTEXITCODE -ne 0) { throw 'Could not enumerate repository files.' }
    $revision = (git -C $repoRoot rev-parse HEAD).Trim()
    $dirty = @(git -C $repoRoot status --porcelain)
    if (-not $AssembleOnly -and $dirty.Count -gt 0) { throw 'Commit the reviewed source and documentation before creating the final archive.' }
    if ($dirty.Count -gt 0) { $revision += ' (working copy; final archive must use committed source)' }
} else {
    $sourceFiles = @()
    foreach ($entry in @('README.md','CHANGELOG.md','.gitignore',$noteName,'Docs','Tools','Neon Relay\00-FOLDER-CONTENTS.txt','Neon Relay\Assets','Neon Relay\Packages','Neon Relay\ProjectSettings')) {
        $item = Join-Path $repoRoot $entry
        if (Test-Path -LiteralPath $item -PathType Container) {
            $sourceFiles += Get-ChildItem -LiteralPath $item -Recurse -File -Force | Where-Object { $_.Name -notlike 'PerformanceTestRun*' } | ForEach-Object { $_.FullName.Substring($repoRoot.Length+1) }
        } elseif (Test-Path -LiteralPath $item) { $sourceFiles += $entry }
    }
}
foreach ($relative in $sourceFiles) {
    $from = Join-Path $repoRoot $relative
    if (-not (Test-Path -LiteralPath $from -PathType Leaf)) { throw "Source file is missing: $relative" }
    $to = Join-Path $sourceDir $relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $to) -Force | Out-Null
    Copy-Item -LiteralPath $from -Destination $to -Force
}
foreach ($file in @('build-result.txt','gameplay-checks.txt','runtime-checks.txt','complete-package-runtime-checks.txt')) {
    $from = Join-Path $repoRoot ('Docs\'+$file)
    if (Test-Path -LiteralPath $from) { Copy-Item -LiteralPath $from -Destination $checksDir -Force }
}
Get-ChildItem -LiteralPath (Join-Path $repoRoot 'Docs') -Filter '*.png' | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $guideDir 'Screenshots') -Force
}
@"
NEON RELAY - COMPLETE PACKAGE

PLAY THE GAME
Double-click NeonRelay.exe in THIS folder.
Unity is NOT required to play. Windows x64 is required.
Extract the entire ZIP before launching. Keep the runtime folders and DLL files beside the executable.
UnityCrashHandler64.exe is a support program; NeonRelay.exe is the game.

CONTROLS
Enter starts/replays. Movement is automatic.
Space, W, Up, or click jumps. Tap again in the air for a double jump.
Hold for height; release early for a short hop. Esc pauses. R restarts the stage.
H opens help/options. M toggles sound. F11 toggles fullscreen.
Reach each finish to advance automatically. All three stages are completable. Bonus cells are optional.

WHAT IS WHERE
01-Game-Guide       Startup, controls, a folder map, and screenshots.
02-Source-and-Build Complete editable source, documentation, Unity settings, and build scripts.
03-Verification    Test results, the source revision, packaging checks, and a file inventory.
NeonRelay_Data     Required game data and compiled scripts. Keep its name.
MonoBleedingEdge   Required C# runtime. Keep its name.
D3D12              Required graphics support files. Keep its name.

Every folder contains 00-FOLDER-CONTENTS.txt explaining what it holds.

EDIT IN UNITY
Unity Hub > Add project from disk > select 02-Source-and-Build\Neon Relay.
Use Unity 6000.5.10f1. Let import finish, then choose Neon Relay > Play game.
The source scene is Assets\Scenes\NeonRelay.unity.

This package contains Neon Relay 1.2.0, the working submission game.
Repository: https://github.com/ogunmefun/2d-project
"@ | Set-Content -LiteralPath (Join-Path $OutputDirectory '00-START-HERE.txt') -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $OutputDirectory '00-START-HERE.txt') -Destination (Join-Path $guideDir 'STARTUP-AND-CONTROLS.txt') -Force
@"
Neon Relay game version: 1.2.0
Package: complete labeled source and Windows player
Source commit: $revision
Repository: https://github.com/ogunmefun/2d-project
The game executable is copied unchanged from the verified Windows build.
Unity regenerates Library/Temp caches; those caches and Git internals are not part of the deliverable.
"@ | Set-Content -LiteralPath (Join-Path $checksDir 'SOURCE-REVISION.txt') -Encoding UTF8

function Get-FolderPurpose([string]$relative) {
    switch -Regex ($relative.Replace('\','/')) {
        '^\.$' { return 'START HERE. NeonRelay.exe launches the game. The numbered folders contain guides, editable source, and verification. Other folders and DLLs support the executable.' }
        '^01-Game-Guide$' { return 'Player instructions and a map of the package. The Screenshots folder shows the game, controls, stages, and results.' }
        '^01-Game-Guide/Screenshots$' { return 'PNG screenshots captured from the verified Windows player. These are reference images; they are not needed to launch the game.' }
        '^02-Source-and-Build$' { return 'The complete clean source repository, with its original folder layout. Open Neon Relay in Unity Hub. README.md, Docs, and Tools explain how to edit, build, and verify it.' }
        '^03-Verification$' { return 'Gameplay and runtime reports, package integrity results, source revision, and FILE-MANIFEST.csv with file sizes and SHA-256 hashes. The manifest excludes itself.' }
        '^D3D12$' { return 'Direct3D graphics support for the Windows player. Keep this folder and its files beside NeonRelay.exe.' }
        '^NeonRelay_Data$' { return 'The Unity player data: scenes, serialized assets, settings, resources, compiled scripts, and native plugins. This exact folder name is required by NeonRelay.exe.' }
        '^NeonRelay_Data/Managed$' { return 'Compiled C# assemblies used by the player. Assembly-CSharp.dll contains the game code; the other DLLs supply Unity and .NET functionality. Editable C# files are in 02-Source-and-Build/Neon Relay/Assets/Scripts.' }
        '^NeonRelay_Data/Plugins(?:/x86_64)?$' { return 'Native 64-bit plugin libraries used by the game runtime, including generated Burst code. Keep these files in their existing locations.' }
        '^NeonRelay_Data/Resources$' { return 'Built-in Unity resources loaded by the Windows player. These support rendering and UI; leave this folder in place.' }
        '^MonoBleedingEdge$' { return 'The bundled Mono runtime that executes the game C# assemblies. No separate Unity or Mono installation is needed to play.' }
        '^MonoBleedingEdge/EmbedRuntime$' { return 'Native Mono runtime libraries loaded by the executable. These DLLs are required support files, not programs to open manually.' }
        '^MonoBleedingEdge/etc$' { return 'Configuration files and subfolders for the bundled Mono runtime. Keep this directory structure intact.' }
        '^MonoBleedingEdge/etc/mono$' { return 'Mono framework configuration, grouped into compatibility profiles (2.0, 4.0, 4.5) and configuration support (mconfig).' }
        '^MonoBleedingEdge/etc/mono/(2\.0|4\.0|4\.5)$' { return 'Configuration for the Mono/.NET compatibility profile named by this folder. These are runtime support settings, not game levels or game versions.' }
        '^MonoBleedingEdge/etc/mono/(2\.0|4\.0|4\.5)/Browsers$' { return 'Standard browser capability definitions distributed with the Mono compatibility profile. They are runtime support data, not saved browser history.' }
        '^MonoBleedingEdge/etc/mono/mconfig$' { return 'Mono configuration templates shipped with the runtime. They are included to preserve the complete Windows player distribution.' }
        default { throw "No contents description defined for folder: $relative" }
    }
}
foreach ($generated in @((Join-Path $guideDir 'FOLDER-MAP.txt'),(Join-Path $checksDir 'PACKAGING-REPORT.txt'),(Join-Path $checksDir 'FILE-MANIFEST.csv'))) {
    if (-not (Test-Path -LiteralPath $generated)) { '' | Set-Content -LiteralPath $generated -Encoding UTF8 }
}
$directories = @((Get-Item -LiteralPath $OutputDirectory)) + @(Get-ChildItem -LiteralPath $OutputDirectory -Recurse -Directory)
foreach ($directory in $directories) {
    $relative = if ($directory.FullName -eq $OutputDirectory) { '.' } else { $directory.FullName.Substring($OutputDirectory.Length+1) }
    $note = Join-Path $directory.FullName $noteName
    if ($relative -eq '02-Source-and-Build' -or $relative.StartsWith('02-Source-and-Build\',[StringComparison]::OrdinalIgnoreCase)) {
        if (-not (Test-Path -LiteralPath $note)) { throw "Source folder needs a tracked contents note: $relative" }
        continue
    }
    $purpose = Get-FolderPurpose $relative
    $items = @(Get-ChildItem -LiteralPath $directory.FullName -Force | Where-Object { $_.Name -ne $noteName } | Sort-Object -Property Name | ForEach-Object {
        if ($_.PSIsContainer) { '[FOLDER] '+$_.Name+' - open its '+$noteName+' for details.' } else { '[FILE]   '+$_.Name }
    })
    @("FOLDER: $relative",'', $purpose,'','CONTENTS',($items -join "`r`n"),'','Game launcher: NeonRelay.exe in the top-level package folder.') |
        Set-Content -LiteralPath $note -Encoding UTF8
}
$map = @('NEON RELAY - FOLDER MAP','','Open NeonRelay.exe at the top level to play.','Each folder has a 00-FOLDER-CONTENTS.txt note.','')
$map += $directories | ForEach-Object {
    if ($_.FullName -eq $OutputDirectory) { '.  (game executable and required runtime)' }
    else { $_.FullName.Substring($OutputDirectory.Length+1) }
} | Sort-Object
$map | Set-Content -LiteralPath (Join-Path $guideDir 'FOLDER-MAP.txt') -Encoding UTF8

# Validate the existing runtime files byte-for-byte after relocation and labeling.
$runtimeFiles = @(Get-ChildItem -LiteralPath $buildDir -Recurse -File | Where-Object { $_.FullName -notmatch 'DoNotShip' })
foreach ($file in $runtimeFiles) {
    $relative = $file.FullName.Substring($buildDir.Length+1)
    $to = Join-Path $OutputDirectory $relative
    if (-not (Test-Path -LiteralPath $to)) { throw "Missing runtime file: $relative" }
    if ((Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $to).Hash) { throw "Runtime file changed: $relative" }
}
foreach ($relative in $sourceFiles) {
    if ((Get-FileHash -LiteralPath (Join-Path $repoRoot $relative)).Hash -ne (Get-FileHash -LiteralPath (Join-Path $sourceDir $relative)).Hash) {
        throw "Source copy mismatch: $relative"
    }
}
if (Get-ChildItem -LiteralPath $OutputDirectory -Recurse -Force | Where-Object { $_.Name -match 'DoNotShip|^\.git$|^AfterHours|^OWNER-' }) {
    throw 'Unexpected private, debug, or unrelated game files in the package.'
}
$unlabeled = @($directories | Where-Object { -not (Test-Path -LiteralPath (Join-Path $_.FullName $noteName)) })
if ($unlabeled.Count) { throw 'One or more folders lack a contents note.' }
@("PASS: NeonRelay.exe is at the top level.","PASS: $($directories.Count) folders each have a contents note.",
    "PASS: $($runtimeFiles.Count) runtime files match the tested Windows build byte-for-byte.",
    "PASS: $($sourceFiles.Count) source files match the source snapshot byte-for-byte.",
    'PASS: Required Unity folder names and dependencies are preserved.',
    'PASS: No editor caches, Git internals, debug output, or After Hours files are included.') |
    Set-Content -LiteralPath (Join-Path $checksDir 'PACKAGING-REPORT.txt') -Encoding UTF8
$manifestPath = Join-Path $checksDir 'FILE-MANIFEST.csv'
Get-ChildItem -LiteralPath $OutputDirectory -Recurse -File -Force | Where-Object { $_.FullName -ne $manifestPath } | Sort-Object FullName | ForEach-Object {
    [PSCustomObject]@{Path=$_.FullName.Substring($OutputDirectory.Length+1).Replace('\','/');Bytes=$_.Length;SHA256=(Get-FileHash -LiteralPath $_.FullName).Hash.ToLowerInvariant()}
} | Export-Csv -LiteralPath $manifestPath -NoTypeInformation -Encoding UTF8
if ($AssembleOnly) { [PSCustomObject]@{Folder=$OutputDirectory;Folders=$directories.Count;RuntimeFiles=$runtimeFiles.Count;SourceFiles=$sourceFiles.Count}; return }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($OutputDirectory,$ArchivePath,[IO.Compression.CompressionLevel]::Optimal,$true)
$zip = [IO.Compression.ZipFile]::OpenRead($ArchivePath)
try {
    $prefix = (Split-Path -Leaf $OutputDirectory)+'/'
    $files = @(Get-ChildItem -LiteralPath $OutputDirectory -Recurse -File -Force)
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($OutputDirectory.Length+1).Replace('\','/')
        $entry = $zip.GetEntry($prefix+$relative)
        if (-not $entry) { throw "Archive missing file: $relative" }
        $stream = $entry.Open(); $sha = [Security.Cryptography.SHA256]::Create()
        try { $packedHash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') }
        finally { $stream.Dispose(); $sha.Dispose() }
        if ($packedHash -ne (Get-FileHash -LiteralPath $file.FullName).Hash) { throw "Archive hash mismatch: $relative" }
    }
    $entryCount = @($zip.Entries | Where-Object { $_.Name }).Count
    if ($entryCount -ne $files.Count) { throw 'Archive contains unexpected files.' }
} finally { $zip.Dispose() }
$archiveHash = (Get-FileHash -LiteralPath $ArchivePath).Hash.ToLowerInvariant()
"$archiveHash  $([IO.Path]::GetFileName($ArchivePath))" | Set-Content -LiteralPath ($ArchivePath+'.sha256') -Encoding ASCII
[PSCustomObject]@{Folder=$OutputDirectory;Archive=$ArchivePath;Bytes=(Get-Item -LiteralPath $ArchivePath).Length;Files=$files.Count;Folders=$directories.Count;SHA256=$archiveHash}
