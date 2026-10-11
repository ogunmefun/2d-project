# Release verification

Version **1.1.0**, verified on October 11, 2026, using Unity **6000.5.10f1** and a Windows x64 standalone player.

- **Unity gameplay checks: 43 passed.** Every sector's route collects its three cells, activates the checkpoint, reaches and solves the relay, respawns with progress preserved, and completes the exit. The route runs actual movement and jump inputs through the same simulation used by the game. Isolated checks cover jump limits, early jump release, solid floors, world boundaries, spikes, pits, laser phases, moving platforms, and locked exits. Added checks verify remote circuit submission is rejected, invalid switch indices are ignored, moving platforms carry a standing robot, and exact floor contact does not eject the robot sideways. The same checks also passed with the installed Mono runtime.
- **Windows executable checks: 30 passed; exit code 0.** Title loading, manual open/back behavior, time frozen in help and pause, saved sound and reduced-effects options, F11 entering/leaving fullscreen, movement, double jump, restart, wrong-answer handling, all three relay interfaces, all sector completion screens, final results, and clean replay. Keyboard actions are queued through Unity's Input System. The reduced-effects check calls the same handler used by its button and checks the saved setting. Previous sound/effects values are restored by the test.
- **Windows build: succeeded, zero errors, zero warnings.**
- **Visual inspection:** title, gameplay, both later sector designs, pause, field manual, relay, sector clear, and final results screens. A tutorial prompt initially overlapped the jumping robot; its position and drawing order were corrected, rebuilt, and rechecked. Final captures render at 1280 × 720 after returning from fullscreen.

The runtime test is opt-in through `-smokeTest`; normal launches start at the title screen and do not run test automation. Game art and sound effects are generated locally. The game has no gameplay server dependency.

The executable UI test positions the robot beside each terminal and sets up completed objectives to isolate transitions/results. It does not claim to traverse the whole game through OS keyboard input. Complete movement routes are covered by the separate deterministic gameplay checks. The results screenshot contains synthetic test times, not a human speedrun. The previous personal best is restored when the test exits.

## Reports and screenshots

- [Gameplay report](gameplay-checks.txt)
- [Executable report](runtime-checks.txt)
- [Build result](build-result.txt)
- [Title screen](title.png)
- [Gameplay](gameplay.png)
- [Relay puzzle](circuit.png)
- [Pause screen](paused.png)
- [Field manual](help.png)
- [Pulse Works](sector-2.png)
- [The Core](sector-3.png)
- [Final relay puzzle](circuit-sector-3.png)
- [Results screen](results.png)

## Coverage boundaries

Windows x64 was built and tested. A WebGL build menu is provided, but WebGL was not part of this release's verification. Other operating systems and controllers were not tested; this release uses keyboard controls with clickable menu and puzzle buttons. Desktop mouse-click automation was unavailable because its permission prompt timed out; keyboard paths and shared option handlers were checked instead. Audio generation/playback paths were exercised without runtime errors, but perceived audio quality was not assessed through listening.

`Tools/package-windows.ps1` excludes Unity's `DoNotShip` debug directories, verifies that the ZIP contains the exact tested game assembly using SHA-256, and writes a SHA-256 sidecar for the download. Test reports contain no build-machine logs or credentials.
