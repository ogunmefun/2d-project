# Neon Relay

A three-stage **2D auto-running platformer** for Windows. A maintenance robot runs through a neon city while you time jumps over spikes, moving saws, and gaps. Finish each course to advance automatically; finish all three to win.

**[DOWNLOAD NEON RELAY — complete game, source, and documentation (ZIP)](https://github.com/ogunmefun/2d-project/releases/download/v1.2.0-submission/NeonRelay-Complete-Package-v1.2.0-submission.zip)**

**No Unity installation, installer, account, or internet connection is needed to play after downloading.** The release is for Windows x64. Use a keyboard and, optionally, a mouse. The game was verified at 1280 × 720 with fullscreen switching; other platforms and gamepad are not verified release targets.

![Neon Relay gameplay](Docs/gameplay.png)

[Download and run](#download-and-run) · [Controls](#controls) · [FAQ](#faq-and-troubleshooting) · [Sharing and submission](#sharing-and-submission) · [Synopsis for the professor](#synopsis-for-the-professor) · [Open and build the source](#open-and-build-the-source)

## Download and run

1. Click **DOWNLOAD NEON RELAY** above. Alternatively, open [Releases](https://github.com/ogunmefun/2d-project/releases/latest), expand **Assets**, and download **NeonRelay-Complete-Package-v1.2.0-submission.zip** (about 39 MB).
2. Find the ZIP in Downloads. Right-click it, choose **Extract All**, select a location such as Desktop or Documents, and click **Extract**.
3. Open the extracted **NEON RELAY - COMPLETE PACKAGE** folder.
4. Double-click **NeonRelay.exe** at the top level. If Windows hides file extensions, it may appear as **NeonRelay** with type **Application**. **UnityCrashHandler64.exe** is a support program; the game launcher is **NeonRelay.exe**.
5. Click the game window and press **Enter** or the start button. After **GET READY**, the robot runs automatically. Press **Space** to jump.

Keep **NeonRelay_Data**, **MonoBleedingEdge**, **D3D12**, and the DLL files beside the executable. Extract the entire archive before playing; running an EXE inside the ZIP or copying only the EXE leaves required files behind.

**Use the named Complete-Package ZIP to play.** GitHub's **Code → Download ZIP** and automatic **Source code (zip)** downloads contain the source repository; they do not include the compiled game. See GitHub's [source archive guide](https://docs.github.com/en/repositories/working-with-files/using-files/downloading-source-code-archives) for how source downloads work.

## Controls

| Action | Control |
| --- | --- |
| Run forward | Automatic — no A/D movement needed |
| Start / replay | Enter or the screen button |
| Jump | Space, W, up arrow, or left-click the play area |
| Double jump | Release, then press jump again while airborne |
| Higher jump / short hop | Hold jump for height; release early for a short hop |
| Pause / resume | Escape |
| Restart current stage | R while playing |
| Help and options | H |
| Toggle sound | M |
| Toggle fullscreen | F11 |
| Close the game | Window close button or Alt+F4; Quit is also on the title screen |

## How to play and win

| Stage | What to expect |
| --- | --- |
| **The Rooftops** | The slowest pace, narrow spikes, and short gaps to learn jump timing |
| **Pulse Works** | Faster movement, wider obstacles, and a moving saw |
| **The Core** | The fastest pace, longer double-jump gaps, and two moving saws |

Save your second jump for wider gaps. The two indicators at the bottom right show your remaining jumps. A checkpoint flag around the middle of each stage saves your retry position. Death returns you to the checkpoint or stage start after a short countdown; retries are unlimited.

Energy cells are **optional bonuses**. Cross a finish line, wait through **STAGE CLEAR**, and the next stage begins automatically. The third finish opens the results screen with total and individual stage performance. All three stages are completable using jump timing alone.

The HUD shows progress, time, retries, and cells. Death keeps collected cells. **R** resets the current stage's checkpoint and cells, but time and retries already spent still count. Menus and preparation countdowns do not add run time.

## FAQ and troubleshooting

**Do I need Unity or Unity Hub?**

Only to inspect, edit, or rebuild the source. The Windows game includes its runtime and launches directly from NeonRelay.exe.

**Can I send just the EXE?**

Send the **complete ZIP**. The EXE depends on neighboring folders and DLLs. On your own PC, you can create a shortcut to the EXE while leaving its files together.

**Why is there no EXE in my download?**

You likely downloaded a source archive. Use the Complete-Package ZIP from the download link above or the release's Assets list.

**Windows reports a missing UnityPlayer.dll, data folder, or other file.**

Extract the whole ZIP into a fresh folder, then run the EXE from that folder. If extraction failed or the download was incomplete, download the ZIP again. Keep the runtime folder names and use files from the same release.

**The game opens, but the robot does not move or jump.**

Click inside the game window, press Enter to start, and wait for GET READY to finish. A/D are not movement controls. Press Escape if paused. Switching away from the game pauses it automatically.

**I closed Help and the game is still paused.**

This is intentional when Help was opened during play. Press Escape again to resume once ready.

**How do I get past a wide gap?**

Hold the first jump, release, and press again while airborne. Holding continuously is one jump; the second jump needs a second press. Use the checkpoint and unlimited retries to practice.

**Do I need every energy cell to finish?**

No. Cross the finish line to advance. Cells only improve your collection total.

**Is progress saved when I close the game?**

Sound and reduced-effects preferences and your auto-run personal-best time are saved locally. The current run and checkpoints are not saved across restarts. There is no account or online leaderboard.

**There is no sound, the display feels busy, or I want fullscreen.**

Press M to toggle sound and check Windows volume. Press H for reduced decorative effects. F11 toggles fullscreen; these display options do not change obstacle timings.

**Can I play on a Mac, phone, in a browser, or with a controller?**

This release was built and tested for Windows x64 with keyboard input. Other platforms and gamepad controls are not supplied or verified.

**Unity shows an empty or blue screen.**

Let scripts finish importing, then choose **Neon Relay → Play game**. Use the **Game** view. The scene is **Assets/Scenes/NeonRelay.unity**. If the menu is missing, check the editor version below and Unity's Console for script errors. The packaged Windows EXE can also be played without opening Unity.

**Where do I report another problem?**

Use the repository's [Issues page](https://github.com/ogunmefun/2d-project/issues) or contact the person who provided the game. Include the release name, Windows version, exact error or screenshot, and steps that caused it.

## What is in the download?

| Item in NEON RELAY - COMPLETE PACKAGE | Purpose |
| --- | --- |
| **NeonRelay.exe** | **Double-click to play** |
| **00-START-HERE.txt** | Quick instructions that open in Notepad |
| **01-PROFESSOR-SYNOPSIS.txt** | How the game was created, built, run, and verified |
| **README.md** | This player guide and FAQ, with the professor synopsis below |
| **01-Game-Guide** | Controls, folder map, and screenshots |
| **02-Source-and-Build** | Full editable Unity project, documentation, and build tools |
| **03-Verification** | Test results, source revision, integrity checks, and file manifest |
| **NeonRelay_Data / MonoBleedingEdge / D3D12 / DLL files** | Required support; keep these beside the EXE |
| **00-FOLDER-CONTENTS.txt in every folder** | Explains the folder and lists its contents |

See the [complete package guide](Docs/PACKAGING.md). Runtime folder names are preserved because the game relies on them.

## Sharing and submission

The complete package is **one downloadable ZIP**. To submit to a course system that accepts ZIP attachments, upload **NeonRelay-Complete-Package-v1.2.0-submission.zip** as one file. If links are accepted, submit [this repository](https://github.com/ogunmefun/2d-project) and the [submission release](https://github.com/ogunmefun/2d-project/releases/tag/v1.2.0-submission). Recipients download, extract, and launch the EXE; they do not need to build anything.

The ZIP includes the game and editable source. The separate `.sha256` download is an optional checksum, not another file needed to play. If an upload service limits the file size or rejects ZIPs, use the release link where submission rules allow it.

For the repository administrator: keep source and documentation in the repository and distribute compiled packages as **Release assets**. This package is attached to its release. Future packages should include the EXE and all runtime files together, with the README download link pointing to the matching release.

## Synopsis for the professor

**Neon Relay** is a Unity/C# coursework project built around a complete gameplay loop: start, automatically run, time jumps, retry obstacles, advance through three stages, and reach final results. Its themed courses increase running speed and the difficulty of spikes, gaps, and moving saws. Double jumps, variable jump height, checkpoints, and unlimited retries let the player improve through practice. Optional cells add challenge without blocking completion.

The project began with Unity Hub's **Universal 2D** template and uses **Unity 6.5 (6000.5.10f1)**. Custom C# code defines courses, movement, collision, input, menus, HUD, effects, and synthesized sound. Visual shapes and scenery are generated in code and drawn with Unity IMGUI. Unity's Input System reads keyboard and mouse input. Development and documentation used **AI coding assistance**, with iterative changes and verification; the source and reports document the implementation.

Gameplay runs in a **fixed 120 Hz kinematic simulation** shared by the live game and simulation checks. `LevelCatalog.cs` defines stages and difficulty; `GameSimulation.cs` handles gravity, collision, buffered and double jumps, checkpoints, pickups, and finishes. `NeonRelayGame.cs` and `GamePresentation.cs` connect the simulation to controls, visuals, audio, menus, stage transitions, and results. An earlier connector-puzzle design was revised into the current jump-timing game, which advances automatically at each finish.

The main scene is **Assets/Scenes/NeonRelay.unity**. The custom **Neon Relay → Build Windows (with checks)** menu runs gameplay checks and builds a **Windows x64 standalone player using the Mono backend**. The executable is packaged with Unity's data and runtime dependencies. Players extract the ZIP and launch NeonRelay.exe; reviewers can import the included Unity source to inspect or rebuild it. Unity is needed only for source work.

Recorded reports show **32 simulation checks passed**, **33 Windows executable checks passed**, and a build with **zero errors and zero warnings**. Automated jump-input routes complete all three stages and verify automatic progression and results. The executable passed all 33 checks again from the complete-package folder. This documentation update preserves the tested executable; packaging checks compare runtime files, source copies, and ZIP contents. Results cover the documented Windows configuration rather than every possible device.

The hand-in includes the game, source, scripts, guides, screenshots, build results, and verification reports. A standalone synopsis is available in [Docs/PROFESSOR-SYNOPSIS.md](Docs/PROFESSOR-SYNOPSIS.md) and as **01-PROFESSOR-SYNOPSIS.txt at the top level of the complete package**. See [submission notes](Docs/SUBMISSION.md) for a demonstration sequence and [verification](Docs/VERIFICATION.md) for test scope and evidence.

## Open and build the source

### Open in Unity Hub

1. Install **Unity 6000.5.10f1** through Unity Hub with Windows build support available.
2. Choose **Add project from disk**. From the complete package, select **02-Source-and-Build/Neon Relay**. From a repository checkout, select **Neon Relay**. This folder contains **Assets**, **Packages**, and **ProjectSettings**.
3. Open the project and let Unity import scripts and resolve packages. First-time editor setup and package import may need internet access; playing the downloaded EXE works offline.
4. Choose **Neon Relay → Play game**, click the Game view, and press Enter. This selects the correct scene even if an empty Untitled scene was open.

### Build and verify

Choose **Neon Relay → Build Windows (with checks)**. The output is **Neon Relay/Builds/Windows/NeonRelay.exe**, with support files in the same Windows folder. **Neon Relay → Run gameplay checks** runs simulation checks without building.

For command-line builds, open PowerShell in the source root containing `Tools` and `Neon Relay`, and close this project's Unity editor first. Adjust the editor path if installed elsewhere:

```powershell
New-Item -ItemType Directory -Path .\artifacts -Force | Out-Null
& 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe' -batchmode -quit -projectPath "$PWD\Neon Relay" -executeMethod NeonRelay.Editor.BuildTools.VerifyAndBuild -logFile "$PWD\artifacts\build.log"
```

With the editor open, `./Tools/request.ps1 windows` requests the same build; `./Tools/request.ps1 verify` requests simulation checks.

For opt-in executable checks, use a **new** output directory and keep the game window focused:

```powershell
& '.\Neon Relay\Builds\Windows\NeonRelay.exe' -smokeTest "$PWD\artifacts\runtime-new"
```

Normal play does not create the QA component. The runner drives keyboard inputs through courses and records screenshots/results; test-created personal-best records are restored on exit. Read [VERIFICATION.md](Docs/VERIFICATION.md) for the scope. Package a verified build using `Tools/package-complete.ps1` as described in [PACKAGING.md](Docs/PACKAGING.md).

### Source map

| File under Neon Relay/Assets | Responsibility |
| --- | --- |
| Scripts/LevelCatalog.cs | Courses, obstacles, speed, checkpoints, and finishes |
| Scripts/GameSimulation.cs | Fixed-step movement, collision, jumping, retries, cells, and completion |
| Scripts/NeonRelayGame.cs | Controls, state, stage transitions, rendering, audio, and HUD |
| Scripts/GamePresentation.cs | Scenery, effects, help/options, and results |
| Scripts/RuntimeSmokeTest.cs | Opt-in executable input and UI verification |
| Editor/GameChecks.cs | Simulation and complete jump-route checks |
| Editor/BuildTools.cs | Startup scene, checked builds, and local build requests |

Package dependencies are recorded in `Packages/manifest.json` and `packages-lock.json`. Game visuals and sounds are generated in C#; no external game asset downloads, backend, or API keys are required. See [CHANGELOG.md](CHANGELOG.md) for the development history.
