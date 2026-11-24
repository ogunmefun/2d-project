# Lantern Garden — synopsis for the professor

Lantern Garden is a beginner-friendly 3D exploration and collecting game developed in Unity 6.5 (6000.5.10f1) with C#. The player explores a small garden, collects eight glowing sun crystals, and returns to an arch to finish. The game includes a title screen, movement and jumping, collection feedback, a map, pause/help options, a victory screen, and replay. There are no enemies or time limits. Falling off the island returns the player to the path while preserving collected crystals.

## Objects, prefabs, lighting, and implementation

The saved scene is `UnityProject/Assets/Scenes/LanternGarden.unity`. Ten reusable prefabs demonstrate Unity's object/component workflow: Explorer, SunCrystal, LanternPost, PineTree, RockCluster, SupplyCrate, FlowerPlanter, GardenBench, StepPlatform, and GardenArch. The scene uses prefab instances, transforms, materials, colliders, trigger volumes, a CharacterController, and camera-follow behavior.

Custom C# scripts implement movement, gravity, forgiving jump timing, pickup triggers, gate conditions, respawning, game state, menus, and sound. Most visual assets are Unity primitives or procedurally generated low-poly meshes. A directional sunlight source and lantern point lights cast real soft shadows. Emissive materials distinguish the lanterns and crystals. Players can toggle shadows in Help to compare the result. Dynamic Batching is disabled for Unity 6.5, and garden materials enable GPU instancing.

Development used AI coding and documentation assistance. The lantern application/desktop icon was generated with AI; its prompt and provenance are included in `ICON-PROVENANCE.md`. The source and verification reports describe the delivered implementation. Character-animation and additional-player work from the proposed next-phase checklist is not part of this submission.

## Building and running

The custom **Lantern Garden > Build Windows (with checks)** editor command validates the scene and builds a Windows x64 standalone player with the Mono backend. The complete download contains that executable and its required Unity runtime files. To play, extract the ZIP, open **PLAY THE GAME**, launch **LanternGarden.exe**, and press Enter. Unity is not required to run the executable.

To review the source, add the included **UnityProject** folder through Unity Hub and open it with Unity 6000.5.10f1. Choose **Lantern Garden > Open garden scene** or **Lantern Garden > Play game**. The **Assets/Prefabs** folder exposes the reusable objects, and **Assets/Scripts** contains the gameplay code. Build tools and the beginner's guide are included.

## Recorded verification

The November 23, 2026 build completed with zero errors and zero warnings. It passed 24 editor checks and 36 executable checks. The executable route moved the CharacterController through the garden, collected every crystal through trigger contact, reached victory, and verified replay. Other checks covered jumping and landing, pause/help, collision, shadow toggling, and fall recovery. The quick-start argument was also verified. Screenshots and individual results accompany the source.

The verified target is Windows x64 at 1280 by 720 on the documented system. These results do not claim coverage of every computer, operating system, or input device. There is no multiplayer or saved in-progress walk across application restarts.
