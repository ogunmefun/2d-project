# Neon Relay — professor synopsis

Course: COSC 495

Game version: 1.2.0, auto-run edition

Submission documentation: October 12, 2026

Repository: https://github.com/ogunmefun/2d-project

Editor: Unity 6.5 (6000.5.10f1)

Delivered platform: Windows x64 standalone player

## Project overview

Neon Relay is a 2D auto-running platformer in which a maintenance robot travels through a neon city. The player times jumps over spikes, moving saws, and gaps. The Rooftops, Pulse Works, and The Core provide three stages of increasing speed and difficulty. Crossing each finish automatically advances to the next stage; finishing the third displays final results.

The gameplay loop includes a title screen, control reference, preparation countdown, automatic movement, variable-height double jumps, optional cells, midpoint checkpoints, unlimited retries, pause/resume, stage transitions, results, and replay. Sound and reduced-effects preferences and a personal-best time persist locally. The current run and checkpoints do not persist after closing the application.

All three stages are completable using jump timing alone. Collectibles do not lock the finish. An earlier version used manual movement and connector puzzles; the project was revised into the current auto-run format.

## How it was created

The project began with Unity Hub's Universal 2D template and was developed in Unity 6000.5.10f1 using C#. Custom scripts implement the game, stages, interface, scenery, effects, and sound. Visual shapes are generated at runtime and drawn using Unity IMGUI. Sound effects are synthesized in code, so the game does not depend on downloaded character, scenery, or audio asset packs.

Unity's Input System supplies keyboard and mouse input. Package dependencies, including the Universal Render Pipeline, are recorded in the manifest and lockfile. The main scene, Assets/Scenes/NeonRelay.unity, starts the game component. Editor tooling selects this as the Play-mode startup scene to avoid accidentally running an empty scene.

Implementation and documentation used AI coding assistance. Development involved design revisions, debugging, automated checks, and inspection of screenshots from the Windows executable. The source, changelog, tools, and recorded reports are included for review.

## Code organization

- LevelCatalog.cs defines three 96-unit courses, obstacle placement, cells, checkpoints, finishes, and running speeds of 6.0, 6.6, and 7.2 units per second.
- GameSimulation.cs implements the fixed 120 Hz simulation shared by gameplay and regression checks. It handles gravity, rectangular collision bounds, automatic movement, variable-height double jumps, a brief grace period after leaving an edge, buffered jump input, respawning, cells, and finishes.
- NeonRelayGame.cs reads input and coordinates game states, simulation updates, stage advancement, HUD, rendering, and sound.
- GamePresentation.cs provides scenery, decorative effects, help/options, and results.
- GameChecks.cs checks simulation behavior and complete jump-only routes. RuntimeSmokeTest.cs optionally checks the Windows executable with keyboard events.
- BuildTools.cs supplies editor menus for gameplay checks, correct scene startup, and Windows builds.

The fixed simulation step separates movement and collision from display-frame timing. Checkpoints and short countdowns provide predictable retry points. Results record time, retries, and cells; stage restarts retain time/retry costs already incurred. Decorative effects can be reduced without altering gameplay timings.

## How it was built and packaged

Neon Relay > Build Windows (with checks) configures the main scene and player settings, runs gameplay checks, and calls Unity's build pipeline for Windows x64. It uses the Mono scripting backend. The default window is 1280 x 720, with resizing and fullscreen switching available.

The build produces NeonRelay.exe alongside NeonRelay_Data, MonoBleedingEdge, graphics support, UnityPlayer.dll, and other dependencies. These files are distributed together; the EXE is not self-contained. Unity is not required to play because the player runtime is included.

Tools/package-complete.ps1 assembles the verified player and editable source into NEON RELAY - COMPLETE PACKAGE. The EXE, quick-start note, README, and this synopsis are at the top level. Numbered folders contain guides, source/build files, and verification. Every folder has a 00-FOLDER-CONTENTS.txt note. The source preserves Assets, Packages, ProjectSettings, documentation, and tools; generated caches and Git internals are omitted.

The packager compares runtime files with the verified build and copied source with the recorded snapshot. It creates a SHA-256 manifest and verifies every ZIP entry against the assembled folder. This documentation release retains the tested 1.2.0 executable.

## How to run and review it

Download the Complete-Package ZIP from the GitHub release, extract the whole ZIP, open NEON RELAY - COMPLETE PACKAGE, and double-click NeonRelay.exe. Click the game window and press Enter. The robot runs automatically after the countdown. Space, W, Up, or a left-click in the play area jumps; release and press again in the air to double jump. Hold for height or release early for a short hop. Escape pauses, R restarts the stage, H opens help/options, M toggles sound, and F11 toggles fullscreen.

For source review, add 02-Source-and-Build/Neon Relay from the complete package to Unity Hub, or Neon Relay from the repository. Open with Unity 6000.5.10f1, allow package import to finish, then choose Neon Relay > Play game. Editor setup/import may need internet access; the downloaded standalone player works offline.

To reproduce the build, choose Neon Relay > Build Windows (with checks). Output is Builds/Windows/NeonRelay.exe inside the Unity project. The README supplies command-line equivalents and opt-in executable QA instructions. Keep the test window focused when running automated input checks.

## Verification and scope

Recorded Windows reports show:

- 32 simulation checks passed.
- 33 Windows executable checks passed.
- The build succeeded with zero errors and zero warnings.
- All 33 executable checks passed again from the labeled package folder on October 12, 2026.

Simulation checks cover movement, collision, jumping, checkpoints, retries, cells, and finish state. Each stage has complete jump-input routes with earlier/later timing variants. Executable checks drive keyboard input through all three courses and verify automatic progression, results, replay, menus, and options. A separate isolated state check tests an airborne finish with zero cells; the full-course route checks do not teleport the player.

Verification covers the recorded Windows x64 configuration and keyboard routes, with 1280 x 720 windowed play and fullscreen switching. Mouse jumping uses the same gameplay path but was not separately driven by the automated keyboard runner. macOS, Linux, WebGL, mobile, and gamepad are not verified release targets. There is no online leaderboard or mid-run save across restarts.

Reports are in 03-Verification in the package and Docs in the repository. Docs/VERIFICATION.md details the test scope. This packaging update adds documentation and preserves runtime files byte-for-byte; it does not claim a new gameplay test run for a documentation-only change.
