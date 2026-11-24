# Lantern Garden

A beginner-friendly 3D exploration game made in Unity. Walk around a little garden, collect eight glowing sun crystals, and return to the garden arch. There are no enemies, lives, or time limits. If you fall off the island, you return to the path and keep your crystals.

**[Download the complete Lantern Garden submission ZIP](https://github.com/ogunmefun/2d-project/releases/download/lantern-garden-v1.0.1-submission/LanternGarden-Complete-Submission-v1.0.1.zip)**

The ZIP contains the Windows game, editable Unity project, guides, screenshots, and verification reports. For coursework, submit the ZIP or [this release link](https://github.com/ogunmefun/2d-project/releases/tag/lantern-garden-v1.0.1-submission). GitHub's **Code > Download ZIP** contains the source repository, not the compiled game.

## Download and run

1. Download **LanternGarden-Complete-Submission-v1.0.1.zip** using the link above.
2. Right-click the downloaded file, choose **Extract All**, and finish extracting it.
3. Open **LANTERN GARDEN - COMPLETE PACKAGE**, then **PLAY THE GAME**.
4. Double-click **LanternGarden.exe**, click the game window, and press **Enter**.
5. Use **WASD / arrows** to move and **Space** to jump. Collect all eight crystals, then walk through the arch.

**Windows x64; Unity is not required to play.** Keep the entire PLAY THE GAME folder together. UnityCrashHandler64.exe is a support program, not the launcher. The `.sha256` download is an optional integrity check, not another file needed to play.

For the professor: [project synopsis](Docs/PROFESSOR-SYNOPSIS.md), [verification and scope](Docs/VERIFICATION.md), and [package contents](Docs/SUBMISSION.md).

## Play the game

On the creator's computer, **PLAY - Lantern Garden** on the desktop opens straight into gameplay with the lantern icon. This machine-specific shortcut is not part of the portable download. Recipients can create their own shortcut to the extracted EXE; adding `-quickStart` to the shortcut target arguments skips the title menu.

Open **PLAY THE GAME** and double-click **LanternGarden.exe**. Keep all the other files in that folder beside the executable. Unity is not required for the Windows player.

Press **Enter** to start. Use **WASD or the arrow keys** to move relative to the camera and **Space** to jump. Walking up the shrine steps also works without jumping. Walk into crystals to collect them; the map's gold dots show what remains. When all eight are collected, follow the green map marker to the arch.

| Control | Action |
| --- | --- |
| WASD / arrows | Move |
| Space | Jump |
| Enter | Start / replay |
| Escape | Pause / resume / close help |
| H | Help and options |
| M | Sound on/off |
| F11 | Fullscreen / windowed |

Help includes a shadows switch so you can compare the lighting or reduce graphics load. Shadows start enabled. Changing focus pauses normal play. Closing Help during a walk leaves the game paused; press Escape when ready. Choose **Start a new walk** in Pause to reset the crystals. Closing the game does not save your current walk; sound preference is saved locally.

## Open in Unity

1. In Unity Hub, add the **UnityProject** folder (the one containing Assets, Packages, and ProjectSettings).
2. Open with **Unity 6.5 / 6000.5.10f1** and let import finish.
3. The saved garden opens automatically when Unity starts with an untouched empty scene. You can also choose **Lantern Garden > Open garden scene** or **Lantern Garden > Play game**.
4. Press Enter in the Game view. Stop Play mode before making changes you want to keep.

Unity is required only for inspecting, editing, or rebuilding the source. First-time package import may require internet access; the standalone Windows game runs offline.

This is a new, separate 3D project. It uses Unity's built-in render pipeline and legacy keyboard input. It does not need external art packs, online services, or a paid package.

### Dynamic Batching warning

Unity 6.5 is retiring Dynamic Batching, an older way of combining draw calls on the CPU. Version 1.0.1 disables it and enables GPU instancing on the garden materials. This warning was unrelated to the empty Untitled scene. The startup fix opens the saved scene without replacing unsaved work. The desktop shortcut launches the standalone game directly; it does not add a Windows login startup task.

## Learn from the project

- **Assets/Prefabs:** ten reusable objects you can drag into the scene: Explorer, SunCrystal, LanternPost, PineTree, RockCluster, SupplyCrate, FlowerPlanter, GardenBench, StepPlatform, and GardenArch.
- **Assets/Materials:** editable surface colours and emissive crystal/lantern materials.
- **Assets/Meshes:** simple faceted crystal and tree meshes.
- **Assets/Scenes:** the saved, editable 3D garden with connected prefab instances and named hierarchy groups.
- **Assets/Scripts:** commented gameplay, camera, collection, menu, and verification code.
- **Assets/Editor:** the scene generator, prefab authoring, validation, and build commands.

Read [the beginner's guide](Docs/BEGINNER-GUIDE.md) for a prefab exercise, lighting/shadow settings, and the source map.

## Build and verify

In the editor choose **Lantern Garden > Validate scene and prefabs**, then **Lantern Garden > Build Windows (with checks)**. Windows output goes to **PLAY THE GAME/LanternGarden.exe** beside the Unity project. The editor tool preserves an existing scene when building; it does not regenerate your edits.

From the Lantern-Garden root in PowerShell, with this project's editor closed:

```powershell
./Tools/build.ps1
```

The opt-in executable checks use the real CharacterController and collectible trigger collisions to complete the garden. Start them from this folder:

```powershell
& '.\PLAY THE GAME\LanternGarden.exe' -gardenSmokeTest "$PWD\artifacts\runtime-new" -logFile "$PWD\artifacts\runtime-new.log"
```

Use a fresh output directory. The test runner does not exist during normal play. Its route uses scripted movement through the same movement code; manual keyboard checks are documented separately. Verification reports and screenshots are in **Docs**.

## Share it

The Windows ZIP contains the complete player and launch instructions. Extract the whole ZIP before starting the EXE. The separate Unity source ZIP contains Assets, Packages, ProjectSettings, docs, and tools; Unity regenerates its import caches. Do not send the EXE alone.

## Troubleshooting

- **Missing DLL or data folder:** extract the complete Windows ZIP into a fresh folder and keep the runtime files together.
- **No movement:** click the game, press Enter, and use Escape if paused. WASD movement begins after starting the walk.
- **Can't find a crystal:** check the gold dots on the map. One crystal is on the central shrine; walk up its front steps.
- **Arch is still closed:** collect all eight crystals. The arch opens automatically; no interaction key is needed.
- **Performance:** try turning shadows off in Help, or use windowed mode. The verified target is Windows x64; other platforms are not supplied or verified.
- **Blank editor view:** open Assets/Scenes/LanternGarden.unity and use the Game view. Check the Console for script errors before playing.
- **Hub shows a render-pipeline notice:** this project intentionally uses the built-in renderer in Unity 6000.5.10f1. Hub labels that renderer deprecated; the included project and player were built and tested with this editor. The notice is separate from a script compilation error.

Created with custom C# code and procedural primitive/mesh artwork, with AI coding assistance. See the included verification report for the actual tested scope.
