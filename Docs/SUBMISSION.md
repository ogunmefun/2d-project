# Neon Relay — submission guide

**Version:** 1.1.0 • **Engine:** Unity 6.5 / 6000.5.10f1 • **Platform:** Windows x64

**Project repository:** https://github.com/ogunmefun/2d-project  
**Playable release:** https://github.com/ogunmefun/2d-project/releases/tag/v1.1.0

## Game concept

Neon Relay is a compact 2D precision platformer about a maintenance robot restoring power to a city. Traverse three sectors, recover nine energy cells, solve three routing puzzles, and bring the grid back online. The challenge combines movement, hazard timing, and deductive reasoning. The entire playfield remains visible so the player can plan jumps before committing.

### Design pillars

- **Precise, forgiving movement:** acceleration and braking, two jumps, variable jump height, a short coyote window, and buffered jump input.
- **Readable hazards:** spikes and moving saws have clear silhouettes; timed lasers show SAFE, WAIT, or LIVE text and cycle progress bars.
- **Fast recovery:** unlimited retries and a mid-sector checkpoint preserve collected cells and solved relay power. Resetting a sector deliberately clears its objectives, while elapsed time and retries remain in the run total.
- **Visible accomplishment:** energy collection produces sound and sparks; checkpoint activation is announced; restored sectors light city windows. A final breakdown shows sector times and retries.

## Progression

| Sector | Identity | Challenge | Relay reasoning |
| --- | --- | --- | --- |
| 01 — The Rooftops | Teal rooftop antenna field | Learn double jumps, cells, gaps, spikes, and the checkpoint | Two live lines, with the middle damaged |
| 02 — Pulse Works | Violet factory piping | A moving ledge and a timed laser require patience | Two adjacent live lines, including the right line |
| 03 — The Core | Amber central reactor | Two offset laser cycles and two moving ledges | Left/right differ, middle matches left, two live lines |

Sectors share a readable rooftop route, with escalating hazards. The first jump includes an on-screen double-jump prompt. Each circuit has an optional hint and unlimited safe attempts.

## Suggested demonstration

1. Start the Windows executable. Open **How to play** to show controls and options; return and start with **Enter**.
2. Use **A/D** and press **Space twice** to reach the first cell. Release jump early to demonstrate height control.
3. Reach the central flag. Touch a hazard or fall to demonstrate safe checkpoint respawning with retained cells.
4. Recover the remaining cells, stand near the relay, and press **E**. Read the clue, toggle **1/2/3**, then press **Enter**. The first answer is left and right on.
5. Enter the exit. Subsequent sectors introduce moving platforms and laser timing. Use **Esc** to pause and **H** for help/options.
6. Complete the final sector to display the lit city and per-sector run summary. Replay starts a fresh run; the personal best remains saved.

There is no required time limit and no lives limit. Faster completion is an optional replay goal.

## Hand-in contents

- **Windows ZIP:** standalone executable, required data/DLLs, and PLAYME instructions. Extract the whole folder before running; Unity is not needed to play.
- **Repository/source archive:** `Neon Relay/Assets`, `Packages`, and `ProjectSettings` form the importable Unity project. Choose that folder through Unity Hub's **Add project from disk**.
- **README:** controls, launch instructions, build commands, and architecture map.
- **This document:** concept, gameplay progression, demonstration sequence, and submission contents.
- **VERIFICATION.md and text reports:** verification scope, actual test results, and screenshots.
- **CHANGELOG.md:** changes in the polished release.

Open `Assets/Scenes/NeonRelay.unity` in the editor. Build with **Neon Relay → Build Windows (with checks)**. Generated caches and local build folders are intentionally excluded from the source archive.

## Implementation and attribution

The project began with Unity Hub's Universal 2D template. A custom fixed 120 Hz kinematic simulation handles axis-aligned platform collision, jumps, moving platform carry, hazards, checkpoint state, collectible state, and exit conditions. The rendering/input layer uses Unity IMGUI and the Input System. Separating the simulation makes complete routes reproducible in regression checks without depending on rendering frame rate.

All game geometry, robot art, scenery, UI shapes, particle effects, and synthesized sound effects are generated in C#. Unity packages are listed and locked in `Packages`. No paid asset packs, external art downloads, backend, account, or API key are required. Implementation and documentation were developed with AI coding assistance; disclose that assistance according to your course's requirements.

## Scope

The tested deliverable is the Windows x64 keyboard-and-mouse version. WebGL has a build command but is not a verified release target. There is no gamepad/mobile input, online leaderboard, or mid-run save across application restarts. Sound and reduced-effects preferences and personal best time persist locally. This package documents the implemented game; any course-specific cover sheet or rubric must be supplied separately.
