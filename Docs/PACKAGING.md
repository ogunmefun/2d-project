# Complete labeled Neon Relay package

[Download the complete package](https://github.com/ogunmefun/2d-project/releases/download/v1.2.0-complete/NeonRelay-Complete-Package-v1.2.0.zip).

Extract the entire ZIP. Open the **NEON RELAY - COMPLETE PACKAGE** folder and double-click **NeonRelay.exe**, which is at the top level. Unity is not required to play.

## Folder layout

| Item | Purpose |
| --- | --- |
| **NeonRelay.exe** | **The game: double-click this file to play.** |
| 00-START-HERE.txt | Launch instructions, controls, and navigation |
| 00-FOLDER-CONTENTS.txt | A note in this folder and every subfolder explaining its contents |
| 01-Game-Guide | Player instructions, a folder map, and screenshots |
| 02-Source-and-Build | Complete editable Unity source, project settings, original documentation, and build scripts |
| 03-Verification | Gameplay/runtime reports, packaging checks, source revision, and a SHA-256 file inventory |
| NeonRelay_Data | Required game data, compiled scripts, plugins, and Unity resources |
| MonoBleedingEdge | Required bundled C# runtime |
| D3D12 | Required Windows graphics support |
| UnityPlayer.dll and other DLLs | Required player support files beside the executable |
| UnityCrashHandler64.exe | Unity's support program; launch NeonRelay.exe to play |

The numbered folders are labeled for browsing. Unity's required runtime names are preserved so the executable works. Each runtime folder has its own contents note explaining what its files do.

## What “complete” includes

The Windows player and all its dependencies, all tracked source files, Unity Assets and metadata, package manifest/lockfile, ProjectSettings, build tools, controls, submission notes, screenshots, changelog, and verification reports. The source repository layout is preserved inside 02-Source-and-Build, so its links and build scripts still work.

Unity regenerates Library and Temp caches on import. Those caches, previous builds, debug output marked DoNotShip, private Git internals, and the separate After Hours game are excluded.

In Unity Hub, select **02-Source-and-Build/Neon Relay** and use **6000.5.10f1**. After import, choose **Neon Relay → Play game**.

## Reproduce the package

Build and verify the Windows player first. From the repository root:

```powershell
# Assemble a labeled folder beside the repository, leaving time for launch verification.
./Tools/package-complete.ps1 -AssembleOnly

# After review and committing source/documentation, create and verify the ZIP.
./Tools/package-complete.ps1 -ArchiveExisting
```

Output defaults to **NEON RELAY - COMPLETE PACKAGE** beside the repository. The ZIP is **NeonRelay-Complete-Package-v1.2.0.zip** in the same parent folder. Use `-OutputDirectory` and `-ArchivePath` to choose new locations. The script preserves existing archives and refuses accidental replacement.

The script checks that every directory contains a note, every runtime file matches the verified build, and every source file matches the exported snapshot. It hashes every file in the finished ZIP against the assembled folder and writes a checksum sidecar. 03-Verification/FILE-MANIFEST.csv lists relative paths, sizes, and SHA-256 hashes; it excludes its own row to avoid a circular checksum.

This packaging release uses the unchanged Neon Relay 1.2.0 game executable. The GitHub tag **v1.2.0-complete** identifies the labeled source/documentation snapshot.

## Verification of this package

On 2026-10-12, the player was launched directly from the new top-level NeonRelay.exe with all folder notes present. **All 33 executable checks passed**, including complete jump-input routes through all three stages, automatic stage advancement, results, replay, and a finish with zero bonus cells. See the [relocated-player report](complete-package-runtime-checks.txt).

The assembled folder contains **33 labeled folders**. All **174 runtime files** were compared byte-for-byte with the tested Windows build. The source snapshot is copied without changing its original file contents. The packaging script checks every archived file against the assembled folder before completing the ZIP.
