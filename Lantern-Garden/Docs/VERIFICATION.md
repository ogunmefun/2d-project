# Lantern Garden 1.0.1 — verification

Verified November 23, 2026, with Unity 6000.5.10f1 on Windows x64. The player used Direct3D 11 on the available Intel Iris Xe graphics device.

The 1.0.1 update disables deprecated Dynamic Batching, enables GPU instancing on garden materials, adds a generated lantern application/desktop icon, opens the saved scene when the editor has an untouched empty scene, and supports `-quickStart` to enter gameplay immediately. Both local shortcuts target the current executable, use the ICO, and pass this option. Their saved target, arguments, icon path, and target existence were read back after saving. The executable verification below ran with `-quickStart`.

New captures: [direct startup](Screenshots-v1.0.1/01-quick-start.png) and [completed garden](Screenshots-v1.0.1/07-garden-restored.png). The title and manual Enter-key verification described below were performed on 1.0.0; the new run verifies direct gameplay startup and safe return to pause from Help.

| Check | Result |
| --- | --- |
| Scene, prefab, material, lighting, and batching validation | 24 passed |
| Windows executable gameplay checks | 36 passed |
| Final Windows build | Succeeded, zero errors and zero warnings |
| Visual inspection | New quick-start gameplay and victory screenshots inspected; original release screenshots retained |
| Direct startup | Executable entered gameplay with the desktop shortcut's `-quickStart` argument |

Reports: [editor-checks.txt](editor-checks.txt), [runtime-checks.txt](runtime-checks.txt), and [build-result.txt](build-result.txt).

## What was tested

Scene checks validate ten real prefab assets, connected explorer/crystal instances, eight uniquely identified crystals, CharacterController step height, trigger setup, solid ground beneath every collectible, supported materials, the gate barrier, real shadow-casting directional and point lights, shadow quality, and the included main scene.

Executable checks verify title/help/start, movement, jumping and landing, pause and frozen walk time, safe return from Help, sound, shadows on/off, and a complete winning route. The route drives the same CharacterController movement code used in gameplay and collects all eight crystals through physics trigger contact. It walks up the shrine steps and reaches the arch without teleporting or directly granting pickups. It then checks replay and renewed collection.

Separate isolated checks verify that a crystal cannot be counted twice, victory requires both the crystals and proximity to the arch, a forced fall returns the player to the starting path while retaining collected crystals, and a solid crate blocks movement. No runtime exceptions or errors were recorded.

The opt-in runner supplies movement input programmatically. It is not a claim that every key was driven by automated Windows keyboard events. A normal non-test launch and Enter key were separately checked through Windows, and the live gameplay session displayed successful collection. During ordinary play, the test component is not created.

## Visual checks

The final screenshots show real 3D geometry, distinct prefab objects, sun and lantern illumination, and cast shadows. The shadow-off comparison removes the cast shadows while preserving the scene. A ground-surface overlap found during the first visual inspection was corrected; the final screenshots show the surfaces without the depth flicker, and all 36 executable checks passed again on that final build.

- [Title](Screenshots/01-title.png)
- [Help and options](Screenshots/02-help.png)
- [Gameplay with shadows](Screenshots/03-garden-shadows.png)
- [Same view with shadows off](Screenshots/04-garden-no-shadows.png)
- [West garden](Screenshots/05-west-garden.png)
- [Shrine after collecting all eight crystals](Screenshots/06-shrine-and-open-gate.png)
- [Victory screen](Screenshots/07-garden-restored.png)

## Scope and reproduction

The verified release target is Windows x64, with the player rendered at 1280 x 720. Other operating systems, mobile, browser deployment, controllers, and every possible monitor/GPU configuration have not been tested. There is no mid-run save across application restarts.

Use the editor menu Lantern Garden > Validate scene and prefabs or Build Windows (with checks). The README includes the optional `-gardenSmokeTest` player command. Use a new output directory and a visible, non-minimized game window when capturing test screenshots. Automated tests ignore focus changes; ordinary play pauses when the game loses focus.

The packaging script checks every ZIP entry against the staged files with SHA-256 hashes. The Windows archive keeps the complete required runtime together. The source archive includes Assets, Packages, ProjectSettings, documentation, and tools; editor caches and generated debug-only folders are excluded.
