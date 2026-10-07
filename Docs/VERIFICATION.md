# Release verification

Verified on October 7, 2026, using Unity **6000.5.10f1** and a Windows x64 standalone player.

- **Unity gameplay checks: 38 passed.** Every sector's route collects its three cells, activates the checkpoint, reaches and solves the relay, respawns with progress preserved, and completes the exit. The route runs actual movement and jump inputs through the same simulation used by the game. Isolated checks cover jump limits, early jump release, solid floors, world boundaries, spikes, pits, laser phases, moving platforms, and locked exits.
- **Windows executable checks: 12 passed.** Title loading, Enter to start, movement, jump, double jump, pause, frozen time while paused, resume, restart, opening a nearby relay, rejection of a wrong answer, and keyboard puzzle completion. The test queues keyboard input through Unity's Input System. It positions the player near the terminal only for the isolated relay UI checks.
- **Windows build: succeeded, zero errors, zero warnings.**
- **Visual inspection:** title, gameplay, pause, and relay screens render at 1280 × 720. The design canvas scales with the window.

The runtime test is opt-in through `-smokeTest`; normal launches start at the title screen and do not run test automation. Game art and sound effects are generated locally. The game has no gameplay server dependency.

## Reports and screenshots

- [Gameplay report](gameplay-checks.txt)
- [Executable report](runtime-checks.txt)
- [Build result](build-result.txt)
- [Title screen](title.png)
- [Gameplay](gameplay.png)
- [Relay puzzle](circuit.png)
- [Pause screen](paused.png)

## Coverage boundaries

Windows x64 was built and tested. A WebGL build menu is provided, but WebGL was not part of this release's verification. Other operating systems and controllers were not tested; this release uses keyboard controls with clickable menu and puzzle buttons.
