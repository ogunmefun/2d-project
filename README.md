# Neon Relay

A small, complete 2D precision platformer built in **Unity 6.5 (6000.5.10f1)** from Unity Hub's Universal 2D template.

You are a little maintenance robot bringing a dark city back online. Recover three energy cells in each sector, solve its power-routing clue at the terminal, and reach the exit. Three stages introduce moving platforms, patrolling saws, spikes, gaps, and timed lasers.

## Play in Unity

1. Clone or download this repository.
2. In **Unity Hub → Add → Add project from disk**, choose the **`Neon Relay`** folder (the folder containing `Assets`, `Packages`, and `ProjectSettings`).
3. Open with **6000.5.10f1**. Allow the first package import to finish.
4. Open **`Assets/Scenes/NeonRelay.unity`**, press Unity's **Play** button, then click **Start the run** or press **Enter** in the Game view.

## Controls

| Action | Keyboard |
| --- | --- |
| Move | A / D or left / right arrows |
| Jump; jump again in the air | Space, W, or up arrow |
| Shorter jump | Release the jump key early |
| Connect to a nearby relay | E |
| Toggle relay lines | 1 / 2 / 3, or click the switches |
| Submit routing / continue | Enter, or click the button |
| Pause / close relay panel | Escape |
| Restart current sector | R |
| Toggle sound | M |

The two dots in the bottom right show your available jumps. Touch the flag on the middle rooftop to set a checkpoint. Falls and hazards return you there and preserve collected cells and restored relay power. You have unlimited retries. Restarting a sector resets its collectibles and checkpoint; its elapsed time and retries still count toward the run.

Laser labels communicate their state as well as color: **SAFE** is safe to cross, **WAIT** warns of the next pulse, and **LIVE** is harmful. The game pauses when the window loses focus. Personal best completion time and sound preference are saved locally.

## Build and verify

In Unity, use the **Neon Relay** menu:

- **Run gameplay checks** runs deterministic movement, collision, hazard, checkpoint, circuit, and three complete level-route checks.
- **Build Windows (with checks)** produces `Neon Relay/Builds/Windows/NeonRelay.exe` and the companion data files.
- **Build WebGL (with checks)** builds to `Neon Relay/Builds/WebGL` when the matching WebGL module is installed. Serve a WebGL build through HTTP rather than opening its HTML as a local file.

The checks traverse every sector with actual movement and jump input, collect every cell, solve every circuit, deliberately respawn at each checkpoint, then reach each exit. Additional isolated checks place the player at edge cases to test spikes, pits, locked exits, and jump limits.

For an already open editor, PowerShell can request these actions:

```powershell
.\Tools\request.ps1 verify
.\Tools\request.ps1 windows
```

Results appear in the ignored `artifacts` folder. With the editor closed, batch mode is also supported:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe' `
  -batchmode -quit -projectPath "$PWD\Neon Relay" `
  -executeMethod NeonRelay.Editor.BuildTools.VerifyAndBuild `
  -logFile "$PWD\build.log"
```

## Project layout

| File | Purpose |
| --- | --- |
| `Assets/Scripts/GameSimulation.cs` | Fixed 120 Hz movement, collisions, double jump, checkpoints, hazards, and win conditions |
| `Assets/Scripts/LevelCatalog.cs` | The three stage layouts, moving platforms, laser timing, and circuit clues |
| `Assets/Scripts/NeonRelayGame.cs` | Keyboard input, procedural 2D drawing, menus, HUD, generated sound effects, and local best time |
| `Assets/Editor/GameChecks.cs` | Deterministic gameplay regression and complete-route checks |
| `Assets/Editor/BuildTools.cs` | Main scene setup, build configuration, editor menus, and batch build entry points |

The game uses a custom axis-aligned kinematic simulation and Unity's IMGUI drawing, with a fixed 1280 × 720 design canvas that scales to the window. Geometry, robot art, UI shapes, and audio are generated in code. No paid assets, API keys, server, or downloaded art are required. Unity's Input System package handles keyboard input.

Unity-generated `Library`, `Temp`, `Logs`, `UserSettings`, and builds are excluded from source control. `.meta` files and the package lock file are included so scene references and package versions are reproducible.
