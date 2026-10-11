# Neon Relay

A complete, three-stage **auto-running 2D platformer** built with Unity 6.5 (6000.5.10f1). Your robot runs forward; you time jumps over spikes, saws, and gaps. Cross a finish line to automatically enter the next stage. Clear all three to win.

![Neon Relay gameplay](Docs/gameplay.png)

## Download and play

[Download Neon Relay 1.2 for Windows x64](https://github.com/ogunmefun/2d-project/releases/download/v1.2.0/NeonRelay-Windows-v1.2.0.zip).

Extract the **whole ZIP**, then open **NeonRelay.exe**. Keep the data folder and DLLs beside it. Unity is not required.

Version 1.2 replaces connector puzzles and manual movement with auto-run, scrolling courses, jump timing, optional bonus cells, stage progress, and automatic transitions. The three stages are all completable. See [submission notes](Docs/SUBMISSION.md) and [verification](Docs/VERIFICATION.md).

Verified: **32 simulation checks**, **33 executable checks**, and a Windows build with **zero errors and warnings**.

## Play in Unity

1. In Unity Hub, choose **Add project from disk** and select the **Neon Relay** folder containing Assets, Packages, and ProjectSettings.
2. Open with **6000.5.10f1** and let script import finish.
3. Choose **Neon Relay → Play game**, then click the Game view and press **Enter**. The editor starts the correct game scene even if an empty Untitled scene was open.
4. If needed, open **Assets/Scenes/NeonRelay.unity** manually. Use the **Game** view, not the phone Simulator.

## Controls

| Action | Control |
| --- | --- |
| Run forward | Automatic |
| Start / replay | Enter or the screen button |
| Jump / double jump | Space, W, up arrow, or click the play area |
| Higher jump | Hold the jump control |
| Short hop | Release early |
| Pause / resume | Escape |
| Restart stage | R |
| Help / options | H |
| Sound | M |
| Fullscreen | F11 |

Tap a second time in the air to cross wide gaps. There are two jump indicators at the bottom right. A short GET READY pause precedes the run and each retry. Pass the checkpoint flag halfway through a stage to save your position. Death keeps bonus cells; restarting a stage resets its checkpoint and collectibles. Retries are unlimited.

Cells are optional. Finishing a stage triggers a short STAGE CLEAR screen and automatically begins the next course. The final finish opens results. The HUD shows stage completion percentage, time, retries, and bonus cells. Time and retries from restarted attempts still count. Menus and countdowns do not add run time.

The game pauses when it loses focus. Closing in-game help leaves it paused until resumed. Sound, reduced effects, fullscreen options, and a separate auto-run personal best are available; old puzzle-mode times do not compete with auto-run times.

## Build and verify

Open the Unity project and choose **Neon Relay → Build Windows (with checks)**. The executable appears at `Neon Relay/Builds/Windows/NeonRelay.exe`.

For command-line builds, close this project's Unity editor first:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe' -batchmode -quit -projectPath "$PWD\Neon Relay" -executeMethod NeonRelay.Editor.BuildTools.VerifyAndBuild -logFile "$PWD\artifacts\build.log"
```

With the editor open, `Tools/request.ps1 windows` requests the same checked build. `verify` runs simulation checks only. Package with `Tools/package-windows.ps1 -Version 1.2.0` after verifying the player.

Opt-in executable QA (use a new output directory and keep the test window focused):

```powershell
& '.\Neon Relay\Builds\Windows\NeonRelay.exe' -smokeTest "$PWD\artifacts\runtime-new"
```

Normal play never creates the QA component. Runtime checks drive keyboard inputs through every course and save screenshots. Synthetic best times are restored on exit.

## Source map

- `LevelCatalog.cs`: three scrolling courses, obstacle placement, speed, checkpoints, and finish lines.
- `GameSimulation.cs`: fixed 120 Hz auto-run, collision, variable-height double jump, retry state, optional cells, and finish detection.
- `NeonRelayGame.cs`: input, stage transitions, audio, HUD, menus, and world rendering.
- `GamePresentation.cs`: scenery, effects, help, and results.
- `GameChecks.cs`: deterministic jump-only route and state regression checks.
- `RuntimeSmokeTest.cs`: opt-in Windows executable keyboard and UI checks.
- `BuildTools.cs`: correct editor startup scene, checked builds, and local build requests.

Artwork, geometry, and sound effects are generated in C#. No external asset downloads, account, backend, or API key are required. Built from Unity Hub's Universal 2D template with AI coding assistance. The verified release target is Windows x64; WebGL, mobile, and gamepad are not verified targets.
