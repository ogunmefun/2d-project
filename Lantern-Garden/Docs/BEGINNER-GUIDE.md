# A beginner's tour of Lantern Garden

## Scene, objects, components, and prefabs

A **scene** is a saved arrangement of game objects. Open Assets/Scenes/LanternGarden.unity to see this garden. The Hierarchy groups the ground, paths, scenery, lighting, collectibles, and player/goal.

A **GameObject** has a Transform that stores its position, rotation, and scale. Components add behaviour: a MeshRenderer draws its surface, a Collider makes it solid or detects contact, a Light illuminates its surroundings, and a C# script supplies game logic.

A **prefab** is a reusable saved object. The objects in Assets/Prefabs are actual Unity prefab assets, and the garden contains linked instances of them. Double-click a prefab to inspect its original; edits to the prefab can update all its instances. An instance can also have its own position, scale, or component overrides.

## Ten prefabs to explore

| Prefab | What to inspect |
| --- | --- |
| Explorer | CharacterController, movement script, and a model made of simple shapes |
| SunCrystal | Faceted mesh, rotation/bob animation, trigger collider, kinematic Rigidbody, and point light |
| LanternPost | Post, frame, emissive material, and real point light with soft shadows |
| PineTree | Solid trunk and two faceted cone canopies that cast shadows |
| RockCluster | Rotated stones with colliders |
| SupplyCrate | Solid box, wood material, and metal bands |
| FlowerPlanter | Box, flower stems, and coloured petals |
| GardenBench | Seat, back, and legs with solid colliders |
| StepPlatform | A low step that the CharacterController can climb |
| GardenArch | Stone pillars, eight light runes, locked barrier, beacon, and goal script |

## First exercise: add a tree

1. Stop Play mode so changes are saved in the scene.
2. In Project, open Assets/Prefabs. Drag PineTree into the Scene view.
3. Set the Transform position to an open spot, such as X = -10, Y = 0, Z = 5. Keep the main paths clear.
4. Change its Y rotation and try a uniform scale of 0.8 or 1.2.
5. Save the scene and press Play. Walk around it and look for its shadow.

Try the same with a crate or bench. Duplicate scenery freely. For the supplied game and verification, keep the eight existing crystals, their unique IDs, and the Game manager's crystal references. If you design a different crystal count, update that array, tutorial text, and verification expectations together.

## Lights and shadows

The **04 - Lighting and soft shadows** hierarchy group contains the dusk sun and six lantern prefab instances. The sun is a Directional Light, which illuminates the scene from one direction. Each lantern contains a Point Light with a range and warm colour. The arch also has a beacon. The small crystal lights add local colour.

The sun and lanterns use **Soft Shadows**. The player, trees, crates, benches, rocks, and ground meshes have shadow casting/receiving enabled. Quality settings enable shadows, a 70-unit shadow distance, and a high-resolution shadow map. Ambient sky/equator/ground colours provide gentle fill light so shadows remain readable.

An emissive material makes a surface look bright; the Light component is what illuminates nearby objects in this scene. Both are used on the lanterns. These are real-time lights, so no lightmap bake is needed before playing.

Select the sun and rotate it to change shadow direction. Select a lantern's Light child and change its intensity or range. Then compare **Shadows: On/Off** in the game's Help panel. More shadow-casting lights cost more rendering time, so avoid duplicating dozens of them at once.

## Where the rules live

| Script | Responsibility |
| --- | --- |
| ExplorerController.cs | Camera-relative WASD/arrows, collision, jump buffering, gravity, and recovery position |
| CrystalPickup.cs | Floating crystal animation and trigger contact |
| GardenGate.cs | Locked barrier, lighting feedback, and nearby exit detection |
| GardenGame.cs | Game states, counting crystals, starting, pausing, winning, rescue, and sound |
| GardenInterface.cs | Title, HUD, map, help, pause, and victory screens |
| GardenCamera.cs | A smooth, fixed-angle camera that follows the explorer |
| GardenSmokeTest.cs | Optional executable verification; not present in ordinary play |
| Editor/GardenBuilder.cs | Creates initial assets, checks scene setup, and builds the player |

To change movement, select Explorer in the scene and adjust Move Speed or Jump Height in the Inspector. Try small changes and test the shrine steps and crates. To change colours, edit a material under Assets/Materials. Save before building.

## Rebuilding

Use Lantern Garden > Build Windows (with checks). The build uses the existing saved scene, so your edits remain. Do not have the same project open in the editor and in a separate command-line build at the same time. The source includes the scene and prefab assets; the procedural builder is not required at normal game startup.
