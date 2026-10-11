# Neon Relay 1.2 — submission notes

## Concept

A small maintenance robot races through a neon city. The player controls jump timing while the robot runs forward automatically. The challenge is reading the next obstacle, choosing short or full-height jumps, and reserving an air jump for longer gaps.

## Gameplay

Three 96-unit scrolling courses form one run. Reaching the finish advances automatically after a short clear screen. The final finish wins. There are no connector puzzles, required collectibles, or manual movement controls.

| Stage | Appearance | Challenge |
| --- | --- | --- |
| The Rooftops | Teal city rooftops | Gentle speed, narrow spikes, short gaps |
| Pulse Works | Violet machinery | Faster running, wider spikes, a moving saw |
| The Core | Amber reactor | Fastest pace, long double-jump gaps, two moving saws |

Every stage is completable using jump timing alone. Optional cells reward jumping routes but never lock the exit. Each stage has a midpoint checkpoint, unlimited retries, and a short preparation pause after death. Run time, retries, and cells appear in the HUD. Final results show total and individual stage performance.

## Demonstration

1. Open NeonRelay.exe and press H to display the controls and options.
2. Return and press Enter. Wait through GET READY; movement begins automatically.
3. Press Space before a spike. Hold for height; tap a second time in the air for a long gap.
4. Pass the checkpoint flag, then miss an obstacle to demonstrate checkpoint retry.
5. Reach the finish and let the game load the next stage without pressing Enter.
6. Complete stage three to display results and the lit city; press Enter to replay.

Escape pauses. H opens help; returning from in-game help keeps the game paused. R resets the stage while preserving time/retry accounting. M toggles sound; F11 toggles fullscreen. Reduced effects are available in the manual.

## Hand-in contents

- Windows ZIP with executable, required data/DLLs, PLAYME, and documentation.
- Importable Unity source: Neon Relay/Assets, Packages, and ProjectSettings.
- README with controls, Unity Hub instructions, build commands, and source map.
- Verification report, generated check outputs, and screenshots.
- Changelog describing the transition to auto-run.

In Unity, choose **Neon Relay → Play game**. The editor uses the main scene automatically, preventing the empty Untitled scene from appearing as a blank game. The scene is also available at Assets/Scenes/NeonRelay.unity.

## Implementation and attribution

A fixed 120 Hz kinematic simulation handles movement, gravity, AABB collision, coyote time, jump buffering, variable-height double jumps, pickups, checkpoints, and finish detection. The camera follows the runner through each course. Unity's Input System supplies controls; IMGUI draws procedural art and menus. Particle bursts and synthesized sounds respond to jumps, deaths, cells, and finishes.

All game visuals and audio are generated in code. The project began with Unity Hub's Universal 2D template. Implementation and documentation used AI coding assistance. Follow your course's disclosure requirements.

The delivered target is Windows x64. No gamepad/mobile controls, online leaderboard, or mid-run save across restarts are included. Local settings and personal best persist. WebGL has a build menu but is not a verified release.
