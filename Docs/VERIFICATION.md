# Verification — Neon Relay 1.2.0

Verified on Windows x64 with Unity 6.5 (6000.5.10f1), Mono scripting backend, 2026-10-11.

| Check | Result |
| --- | --- |
| Fixed-step gameplay regression | 32 passed |
| Windows executable checks | 33 passed |
| Windows build | Succeeded, 0 errors, 0 warnings |

Reports: [gameplay](gameplay-checks.txt), [runtime](runtime-checks.txt), [build](build-result.txt).

## Simulation checks

The actual game simulation runs under Unity and the installed Mono runtime. Tests cover preparation countdown, movement without directional input, floor collision, double-jump limits, variable jump height, missed obstacles, checkpoint crossing while airborne, retained bonus cells, pit death, and frozen completion state.

Each of the three stages is traversed from spawn to finish using **only timed jump inputs**, with no position changes or objective overrides. Three timing variants per stage check earlier and later jumps. All nine routes finish without a death. Separate isolated state checks confirm an airborne finish works with zero bonus cells and no puzzle requirement.

## Executable checks

The opt-in RuntimeSmokeTest uses Unity's Input System to check title/start, manual, sound persistence, reduced-effects handler, fullscreen, automatic movement, Space jump/double jump, pause/resume, frozen pause time, safe return from help, and stage reset. E leaves gameplay active; the connector screen has been removed.

After the menu/input checks, the Windows player completes **all three full courses using keyboard jump events**. No teleportation is used for these routes. It verifies stage-clear screens, automatic advancement without Enter, the final results screen, and clean replay. A separate final check positions the character at the exit to isolate the optional-cell/airborne-finish condition. Test-created personal-best records are restored before exit.

The runtime report contains no failed checks or runtime errors. Screenshots were captured from the built executable. Title, gameplay, help, and results were visually inspected for readability and the new controls/progress display.

## Reproduce

In Unity, choose **Neon Relay → Run gameplay checks** or **Build Windows (with checks)**. For executable QA, launch the player with `-smokeTest` followed by a new output folder, and keep its window focused. The README contains full commands. QA runs only when explicitly requested through that command-line argument.

## Scope

Verified target: Windows x64, keyboard input, 1280×720 windowed and fullscreen switching. Click-to-jump uses the same jump path but was not separately driven by the automated keyboard runner. The editor's startup-scene selection is configured in BuildTools; the main-scene Windows build was tested. This is not a claim of testing every display size, graphics driver, or input device. WebGL, mobile, and gamepad are not verified release targets.

## Complete-package relocation check — 2026-10-12

The unchanged Windows player was copied into the complete labeled package with NeonRelay.exe at the top level. Its executable QA passed all 33 checks again from that location, with a contents note in every folder. See [complete-package-runtime-checks.txt](complete-package-runtime-checks.txt) and the [package guide](PACKAGING.md) for layout and integrity checks.
