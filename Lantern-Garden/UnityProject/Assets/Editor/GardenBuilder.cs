using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace LanternGarden.Editor
{
    [InitializeOnLoad]
    public static class GardenBuilder
    {
        public const string ScenePath = "Assets/Scenes/LanternGarden.unity";
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        static readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        static Mesh coneMesh, gemMesh;
        static GardenBuilder() { EditorApplication.delayCall += () => { if (File.Exists(ScenePath)) EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath); }; }

        [MenuItem("Lantern Garden/Play game")]
        public static void Play() { EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath); EditorApplication.isPlaying = true; }
        [MenuItem("Lantern Garden/Show reusable prefabs")]
        public static void ShowPrefabs() { Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Prefabs"); EditorGUIUtility.PingObject(Selection.activeObject); }

        public static void CreateAndBuild()
        {
            if (!File.Exists(ScenePath)) CreateGarden();
            else EditorSceneManager.OpenScene(ScenePath);
            BuildWindows();
        }

        static void Configure()
        {
            PlayerSettings.companyName = "Ogunmefun"; PlayerSettings.productName = "Lantern Garden"; PlayerSettings.bundleVersion = "1.0.1";
            GardenStartup.ConfigureRenderingAndIcon();
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false; PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
            var settings = new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings"));
            var input = settings.FindProperty("activeInputHandler"); if (input != null) { input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); }
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode3D;
            QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
            QualitySettings.shadows = ShadowQuality.All; QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            QualitySettings.shadowDistance = 70; QualitySettings.shadowCascades = 4;
            QualitySettings.shadowProjection = ShadowProjection.StableFit; QualitySettings.pixelLightCount = 8;
            QualitySettings.antiAliasing = 4; QualitySettings.vSyncCount = 0;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static void CreateGarden()
        {
            foreach (string folder in new[] { "Scenes", "Prefabs", "Materials", "Meshes" }) Directory.CreateDirectory("Assets/" + folder);
            AssetDatabase.Refresh();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateMaterials();
            coneMesh = MakeFacetMesh(false); coneMesh.name = "Low-poly cone"; AssetDatabase.CreateAsset(coneMesh, "Assets/Meshes/PineCone.asset");
            gemMesh = MakeFacetMesh(true); gemMesh.name = "Sun crystal"; AssetDatabase.CreateAsset(gemMesh, "Assets/Meshes/SunCrystal.asset");
            CreatePrefabs();
            CreateWorld();
            Configure();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            AssetDatabase.SaveAssets();
        }

        static void CreateMaterials()
        {
            Mat("Grass", new Color(.22f, .39f, .29f)); Mat("Moss", new Color(.39f, .53f, .34f));
            Mat("Earth", new Color(.19f, .28f, .27f)); Mat("Path", new Color(.72f, .65f, .47f));
            Mat("Stone", new Color(.48f, .57f, .56f)); Mat("DarkStone", new Color(.22f, .31f, .33f));
            Mat("Bark", new Color(.29f, .19f, .12f)); Mat("Pine", new Color(.15f, .33f, .25f));
            Mat("PineTip", new Color(.29f, .47f, .31f)); Mat("Wood", new Color(.52f, .31f, .16f));
            Mat("Metal", new Color(.14f, .23f, .24f)); Mat("Cream", new Color(.94f, .85f, .6f));
            Mat("Coat", new Color(.27f, .62f, .57f)); Mat("Hat", new Color(.92f, .51f, .22f));
            Mat("Water", new Color(.18f, .46f, .49f)); Mat("Flower", new Color(.86f, .44f, .47f));
            Mat("Gold", new Color(1, .73f, .23f), new Color(1, .48f, .06f) * 1.1f);
            Mat("Lamp", new Color(1, .85f, .46f), new Color(1, .59f, .18f) * 2.5f);
            Mat("Open", new Color(.5f, 1, .75f), new Color(.25f, .95f, .58f) * 1.7f);
        }
        static void Mat(string name, Color color, Color? emission = null)
        {
            var mat = new Material(Shader.Find("Standard")) { name = name, color = color };
            mat.SetFloat("_Glossiness", .15f);
            if (emission.HasValue) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", emission.Value); }
            AssetDatabase.CreateAsset(mat, "Assets/Materials/" + name + ".mat"); materials[name] = mat;
        }

        static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, string mat, bool solid = false)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = materials[mat]; renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true;
            if (!solid) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
        static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale, string mat, bool solid = false) { return Shape(PrimitiveType.Cube, name, parent, position, scale, mat, solid); }
        static GameObject MeshObject(string name, Transform parent, Vector3 position, Vector3 scale, Mesh mesh, string mat)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = materials[mat]; renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true;
            return go;
        }
        static Light PointLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range, bool shadows)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = color; light.intensity = intensity; light.range = range;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None; light.shadowBias = .03f; light.shadowNormalBias = .2f;
            light.renderMode = LightRenderMode.ForcePixel; return light;
        }
        static void SavePrefab(GameObject root)
        {
            prefabs[root.name] = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/" + root.name + ".prefab");
            UnityEngine.Object.DestroyImmediate(root);
        }

        static void CreatePrefabs()
        {
            var go = new GameObject("Explorer");
            var motor = go.AddComponent<ExplorerController>(); var controller = go.GetComponent<CharacterController>();
            controller.height = 1.8f; controller.center = new Vector3(0, .95f, 0); controller.radius = .35f; controller.stepOffset = .4f; controller.slopeLimit = 50; controller.minMoveDistance = 0;
            var model = new GameObject("Character model").transform; model.SetParent(go.transform, false); motor.model = model;
            Shape(PrimitiveType.Capsule, "Coat", model, new Vector3(0, .85f, 0), new Vector3(.7f, .48f, .6f), "Coat");
            Shape(PrimitiveType.Sphere, "Head", model, new Vector3(0, 1.53f, .04f), Vector3.one * .58f, "Cream");
            Shape(PrimitiveType.Cylinder, "Hat brim", model, new Vector3(0, 1.76f, .04f), new Vector3(.85f, .055f, .85f), "Hat");
            Shape(PrimitiveType.Cylinder, "Hat crown", model, new Vector3(0, 1.87f, .04f), new Vector3(.5f, .12f, .5f), "Hat");
            Cube("Backpack", model, new Vector3(0, 1.02f, -.33f), new Vector3(.52f, .59f, .23f), "Wood");
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Shape(PrimitiveType.Sphere, "Eye", model, new Vector3(sign * .115f, 1.56f, .304f), new Vector3(.07f, .09f, .035f), "Metal");
                Cube("Boot", model, new Vector3(sign * .19f, .18f, .06f), new Vector3(.25f, .32f, .43f), "Metal");
                Shape(PrimitiveType.Capsule, "Sleeve", model, new Vector3(sign * .42f, .94f, 0), new Vector3(.22f, .25f, .22f), "Coat");
            }
            SavePrefab(go);

            go = new GameObject("SunCrystal");
            var pickup = go.AddComponent<CrystalPickup>();
            Shape(PrimitiveType.Cylinder, "Stone socket", go.transform, new Vector3(0, .13f, 0), new Vector3(.85f, .13f, .85f), "DarkStone");
            pickup.gem = MeshObject("Floating crystal", go.transform, new Vector3(0, 1.05f, 0), Vector3.one, gemMesh, "Gold").transform;
            pickup.glow = PointLight("Golden glow", go.transform, new Vector3(0, .8f, 0), new Color(1, .7f, .25f), 1.5f, 3, false).gameObject;
            var trigger = go.AddComponent<SphereCollider>(); trigger.center = new Vector3(0, .9f, 0); trigger.radius = .8f; trigger.isTrigger = true;
            var body = go.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            SavePrefab(go);

            go = new GameObject("LanternPost");
            Shape(PrimitiveType.Cylinder, "Foot", go.transform, new Vector3(0, .12f, 0), new Vector3(.7f, .12f, .7f), "DarkStone", true);
            Shape(PrimitiveType.Cylinder, "Post", go.transform, new Vector3(0, 1.25f, 0), new Vector3(.17f, 1.2f, .17f), "Metal", true);
            Cube("Warm lantern", go.transform, new Vector3(0, 2.55f, 0), new Vector3(.5f, .65f, .5f), "Lamp");
            Cube("Roof", go.transform, new Vector3(0, 2.93f, 0), new Vector3(.8f, .16f, .8f), "Metal");
            foreach (float x in new[] { -.28f, .28f }) foreach (float z in new[] { -.28f, .28f }) Cube("Lantern frame", go.transform, new Vector3(x, 2.55f, z), new Vector3(.05f, .76f, .05f), "Metal");
            PointLight("Warm light - soft shadows", go.transform, new Vector3(0, 2.55f, 0), new Color(1, .68f, .32f), 3.2f, 7.5f, true);
            SavePrefab(go);

            go = new GameObject("PineTree");
            Shape(PrimitiveType.Cylinder, "Trunk", go.transform, new Vector3(0, 1.1f, 0), new Vector3(.5f, 1.1f, .5f), "Bark", true);
            MeshObject("Lower foliage", go.transform, new Vector3(0, 1.1f, 0), new Vector3(2.2f, 2.9f, 2.2f), coneMesh, "Pine");
            MeshObject("Upper foliage", go.transform, new Vector3(0, 2.5f, 0), new Vector3(1.5f, 2.2f, 1.5f), coneMesh, "PineTip");
            SavePrefab(go);

            go = new GameObject("RockCluster");
            for (int i = 0; i < 3; i++)
            {
                var rock = Cube("Faceted stone", go.transform, new Vector3((i - 1) * .45f, .28f + i * .07f, (i % 2) * .3f), new Vector3(.9f, .6f + i * .12f, .8f), i == 1 ? "DarkStone" : "Stone", true);
                rock.transform.localRotation = Quaternion.Euler(i * 8, 18 + i * 37, 7);
            }
            SavePrefab(go);

            go = new GameObject("SupplyCrate");
            Cube("Crate body", go.transform, new Vector3(0, .55f, 0), new Vector3(1.1f, 1.1f, 1.1f), "Wood", true);
            foreach (float side in new[] { -.5f, .5f }) Cube("Metal binding", go.transform, new Vector3(side, .55f, 0), new Vector3(.09f, 1.15f, 1.15f), "Metal");
            SavePrefab(go);

            go = new GameObject("FlowerPlanter");
            Cube("Planter", go.transform, new Vector3(0, .28f, 0), new Vector3(1.6f, .56f, .65f), "Wood", true);
            for (int i = 0; i < 5; i++)
            {
                Shape(PrimitiveType.Cylinder, "Stem", go.transform, new Vector3(-.6f + i * .3f, .74f, 0), new Vector3(.045f, .24f, .045f), "PineTip");
                Shape(PrimitiveType.Sphere, "Flower", go.transform, new Vector3(-.6f + i * .3f, .98f, 0), Vector3.one * .26f, i % 2 == 0 ? "Flower" : "Cream");
            }
            SavePrefab(go);

            go = new GameObject("GardenBench");
            Cube("Seat", go.transform, new Vector3(0, .7f, 0), new Vector3(2.2f, .18f, .7f), "Wood", true);
            Cube("Back", go.transform, new Vector3(0, 1.16f, .35f), new Vector3(2.2f, .65f, .16f), "Wood", true);
            foreach (float x in new[] { -.8f, .8f }) Cube("Leg", go.transform, new Vector3(x, .35f, 0), new Vector3(.2f, .7f, .6f), "Metal", true);
            SavePrefab(go);

            go = new GameObject("StepPlatform");
            Cube("Walkable step", go.transform, new Vector3(0, .15f, 0), new Vector3(3, .3f, .8f), "Stone", true);
            SavePrefab(go);

            go = new GameObject("GardenArch"); var gate = go.AddComponent<GardenGate>();
            foreach (float x in new[] { -1.8f, 1.8f })
            {
                Cube("Pillar", go.transform, new Vector3(x, 1.85f, 0), new Vector3(.65f, 3.7f, .8f), "Stone", true);
                Cube("Footing", go.transform, new Vector3(x, .2f, 0), new Vector3(1, .4f, 1.1f), "DarkStone", true);
            }
            Cube("Lintel", go.transform, new Vector3(0, 3.75f, 0), new Vector3(4.3f, .7f, 1), "Stone", true);
            var runes = new List<Renderer>();
            for (int i = 0; i < 8; i++) runes.Add(Cube("Light rune " + (i + 1), go.transform, new Vector3(-1.4f + i * .4f, 3.76f, -.53f), new Vector3(.22f, .25f, .08f), "Gold").GetComponent<Renderer>());
            gate.runes = runes.ToArray(); gate.lockedMaterial = materials["Gold"]; gate.openMaterial = materials["Open"];
            gate.seal = new GameObject("Locked gate - disappears with 8 crystals"); gate.seal.transform.SetParent(go.transform, false);
            for (int i = 0; i < 3; i++) Cube("Sealed light bar", gate.seal.transform, new Vector3(0, .6f + i * .7f, 0), new Vector3(3.1f, .1f, .12f), "Gold");
            var barrier = gate.seal.AddComponent<BoxCollider>(); barrier.center = new Vector3(0, 1.3f, 0); barrier.size = new Vector3(3.1f, 2.6f, .4f);
            gate.beacon = PointLight("Exit beacon", go.transform, new Vector3(0, 2.7f, -.7f), new Color(1, .7f, .3f), 1.2f, 8, true);
            SavePrefab(go);
        }

        static Transform Group(string name) { return new GameObject(name).transform; }
        static GameObject Place(string prefab, Transform group, Vector3 position, float rotation = 0, float scale = 1)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[prefab]);
            instance.transform.SetParent(group); instance.transform.position = position; instance.transform.rotation = Quaternion.Euler(0, rotation, 0); instance.transform.localScale = Vector3.one * scale; return instance;
        }
        static void PathBetween(Transform root, Vector2 a, Vector2 b, float width = 2.2f)
        {
            Vector2 delta = b - a;
            var path = Cube("Sandstone path", root, new Vector3((a.x + b.x) / 2, .025f, (a.y + b.y) / 2), new Vector3(width, .05f, delta.magnitude + .3f), "Path");
            path.transform.rotation = Quaternion.Euler(0, Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg, 0);
        }

        static void CreateWorld()
        {
            var land = Group("01 - Ground and walkable steps");
            Cube("Garden island - solid ground", land, new Vector3(0, -.7f, 0), new Vector3(34, 1.4f, 34), "Earth", true);
            Cube("Grass surface", land, new Vector3(0, -.015f, 0), new Vector3(33.9f, .05f, 33.9f), "Grass");
            var paths = Group("02 - Garden paths");
            PathBetween(paths, new Vector2(0, -14), new Vector2(0, 13.5f), 2.8f);
            Vector2[] loop = { new Vector2(0, -5), new Vector2(-7, -9), new Vector2(-12, 0), new Vector2(-8, 8), new Vector2(0, 10), new Vector2(8, 8), new Vector2(11, -1), new Vector2(7, -9), new Vector2(0, -5) };
            for (int i = 0; i < loop.Length - 1; i++) PathBetween(paths, loop[i], loop[i + 1]);
            Cube("Raised sun shrine", land, new Vector3(0, .6f, 3), new Vector3(4.2f, 1.2f, 4.4f), "Stone", true);
            for (int i = 0; i < 3; i++) Place("StepPlatform", land, new Vector3(0, i * .3f, -.8f + i * .6f));
            Cube("Reflecting pond - shallow decoration", land, new Vector3(-5.8f, .06f, 2.2f), new Vector3(4.2f, .1f, 4.5f), "Water");

            var decor = Group("03 - Prefab scenery");
            Vector2[] trees = { new Vector2(-14,-12), new Vector2(-10,-14), new Vector2(-4,-13), new Vector2(13,-13), new Vector2(15,-7), new Vector2(15,0), new Vector2(14,10), new Vector2(11,14), new Vector2(5,14), new Vector2(-5,14), new Vector2(-12,13), new Vector2(-15,6), new Vector2(-15,-4) };
            for (int i = 0; i < trees.Length; i++) Place("PineTree", decor, new Vector3(trees[i].x, 0, trees[i].y), i * 31, .8f + (i % 3) * .16f);
            foreach (Vector2 p in new[] { new Vector2(-15,-9), new Vector2(-15,11), new Vector2(15,13), new Vector2(14,-4), new Vector2(-4,5), new Vector2(-7,4) }) Place("RockCluster", decor, new Vector3(p.x, 0, p.y), p.x * 17);
            Place("SupplyCrate", decor, new Vector3(5, 0, -11), 15); Place("SupplyCrate", decor, new Vector3(9, 0, -12), -10); Place("SupplyCrate", decor, new Vector3(9, 1.1f, -12), 5);
            Place("GardenBench", decor, new Vector3(-7, 0, -2), 70); Place("GardenBench", decor, new Vector3(6, 0, 5), -90);
            foreach (float x in new[] { -3.8f, 3.8f }) Place("FlowerPlanter", decor, new Vector3(x, 0, 11), 0);
            Place("FlowerPlanter", decor, new Vector3(-4.5f, 0, -6), 50); Place("FlowerPlanter", decor, new Vector3(12, 0, -9), 15);
            // Small moss patches break up the ground without adding obstacles to the paths.
            for (int i = 0; i < 16; i++)
            {
                float x = (i % 4) * 8 - 12, z = (i / 4) * 8 - 12;
                Shape(PrimitiveType.Cylinder, "Moss patch", land, new Vector3(x, .021f, z), new Vector3(3.7f, .008f, 2.6f), "Moss");
            }

            var lighting = Group("04 - Lighting and soft shadows");
            var sun = new GameObject("Dusk sun - soft directional shadows").AddComponent<Light>(); sun.transform.SetParent(lighting); sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(42, -32, 0); sun.color = new Color(1, .83f, .63f); sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft; sun.shadowBias = .035f; sun.shadowNormalBias = .2f; sun.shadowStrength = .8f; RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight; RenderSettings.ambientSkyColor = new Color(.45f, .54f, .63f);
            RenderSettings.ambientEquatorColor = new Color(.25f, .35f, .36f); RenderSettings.ambientGroundColor = new Color(.14f, .19f, .2f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.22f, .34f, .38f); RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 40; RenderSettings.fogEndDistance = 100;
            foreach (Vector2 p in new[] { new Vector2(-10,-11), new Vector2(10,-7), new Vector2(-13,4), new Vector2(13,5), new Vector2(-5,11), new Vector2(5,11) }) Place("LanternPost", lighting, new Vector3(p.x, 0, p.y));

            var pickups = Group("05 - Collectible prefab instances");
            Vector3[] points = { new Vector3(-7,0,-9), new Vector3(7,0,-9), new Vector3(11,0,-1), new Vector3(8,0,8), new Vector3(-8,0,8), new Vector3(-12,0,0), new Vector3(0,1.2f,3), new Vector3(0,0,-5) };
            string[] places = { "Cedar path", "Supply corner", "East lantern", "Sunlit terrace", "Quiet grove", "West path", "Sun shrine", "Welcome path" };
            var crystals = new List<CrystalPickup>();
            for (int i = 0; i < points.Length; i++) { var crystal = Place("SunCrystal", pickups, points[i]).GetComponent<CrystalPickup>(); crystal.name = "Crystal " + (i + 1) + " - " + places[i]; crystal.crystalId = i; crystal.locationName = places[i]; PrefabUtility.RecordPrefabInstancePropertyModifications(crystal); crystals.Add(crystal); }
            var gameplay = Group("06 - Player and goal");
            var player = Place("Explorer", gameplay, new Vector3(0, .08f, -13)).GetComponent<ExplorerController>();
            var gate = Place("GardenArch", gameplay, new Vector3(0, 0, 13.5f)).GetComponent<GardenGate>();
            var game = new GameObject("Game manager - start here for rules").AddComponent<GardenGame>(); game.player = player; game.gate = gate; game.crystals = crystals.ToArray();
            var camera = new GameObject("Main Camera").AddComponent<Camera>(); camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 11.5f;
            camera.nearClipPlane = .1f; camera.farClipPlane = 150; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.22f, .34f, .38f);
            camera.allowHDR = true; camera.allowMSAA = true; camera.gameObject.AddComponent<AudioListener>();
            var follow = camera.gameObject.AddComponent<GardenCamera>(); follow.target = player.transform;
            camera.transform.position = new Vector3(10, 19, -17); camera.transform.LookAt(Vector3.zero);
        }

        static Mesh MakeFacetMesh(bool gem)
        {
            int sides = gem ? 6 : 7;
            var vertices = new List<Vector3>(); var indices = new List<int>();
            Action<Vector3, Vector3, Vector3> triangle = (a,b,c) => { int n = vertices.Count; vertices.Add(a); vertices.Add(b); vertices.Add(c); indices.Add(n); indices.Add(n+1); indices.Add(n+2); };
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2 / sides, b = (i + 1) * Mathf.PI * 2 / sides, radius = gem ? .38f : 1;
                Vector3 p = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius), q = new Vector3(Mathf.Cos(b) * radius, 0, Mathf.Sin(b) * radius);
                triangle(p, Vector3.up * (gem ? .7f : 1), q);
                triangle(q, gem ? Vector3.down * .55f : Vector3.zero, p);
            }
            var mesh = new Mesh(); mesh.SetVertices(vertices); mesh.SetTriangles(indices, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        [MenuItem("Lantern Garden/Validate scene and prefabs")]
        public static void Validate()
        {
            var lines = new List<string>();
            Action<bool,string> check = (ok, name) => { if (!ok) throw new Exception("FAIL: " + name); lines.Add("PASS: " + name); };
            var game = UnityEngine.Object.FindAnyObjectByType<GardenGame>();
            check(game != null && game.player && game.gate, "Scene has a game manager, explorer, and exit arch.");
            check(game.crystals.Length == 8 && game.crystals.All(c => c != null), "Eight collectible prefab instances are assigned.");
            check(game.crystals.Select(c => c.crystalId).Distinct().Count() == 8, "Every crystal has a unique ID.");
            check(AssetDatabase.FindAssets("t:Prefab", new[] {"Assets/Prefabs"}).Length == 10, "Ten reusable prefab assets exist.");
            check(game.crystals.All(c => PrefabUtility.IsPartOfPrefabInstance(c)), "Crystals retain their prefab connections.");
            check(PrefabUtility.IsPartOfPrefabInstance(game.player), "Explorer is an editable prefab instance.");
            check(game.player.GetComponent<CharacterController>().stepOffset >= .3f, "Player can walk up the shrine steps.");
            check(game.crystals.All(c => c.GetComponent<SphereCollider>().isTrigger && c.GetComponent<Rigidbody>().isKinematic), "Collectibles use trigger colliders with kinematic bodies.");
            check(RenderSettings.sun && RenderSettings.sun.shadows == LightShadows.Soft, "Directional sunlight casts soft shadows.");
            check(UnityEngine.Object.FindObjectsByType<Light>().Count(l => l.type == LightType.Point && l.shadows == LightShadows.Soft) >= 6, "Lanterns have real point lights with soft shadows.");
            check(QualitySettings.shadows == ShadowQuality.All && QualitySettings.shadowDistance >= 50, "Player quality settings enable visible real-time shadows.");
            check(UnityEngine.Object.FindObjectsByType<MeshRenderer>().All(r => r.sharedMaterial && r.sharedMaterial.shader && r.sharedMaterial.shader.isSupported), "Every mesh has a supported material.");
            check(game.gate.seal.GetComponent<BoxCollider>() != null, "The closed exit has a physical barrier.");
            Physics.SyncTransforms();
            foreach (var crystal in game.crystals)
                check(Physics.Raycast(crystal.transform.position + Vector3.up * 4, Vector3.down, 6, ~0, QueryTriggerInteraction.Ignore), "Solid ground below " + crystal.locationName + ".");
            check(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == ScenePath), "Main garden scene is included in the build.");
            check(GardenStartup.DynamicBatchingDisabled(), "Deprecated dynamic batching is disabled for Windows.");
            check(AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" }).All(id => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(id)).enableInstancing), "Garden materials support GPU instancing.");
            Directory.CreateDirectory(Path.Combine(Root, "Docs")); File.WriteAllLines(Path.Combine(Root, "Docs/editor-checks.txt"), lines);
            Debug.Log("GARDEN_CHECKS: " + lines.Count + " passed.");
        }

        [MenuItem("Lantern Garden/Build Windows (with checks)")]
        public static void BuildWindows()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != ScenePath) EditorSceneManager.OpenScene(ScenePath);
            Configure(); Validate(); AssetDatabase.SaveAssets();
            string output = Path.Combine(Root, "PLAY THE GAME/LanternGarden.exe"); Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, target = BuildTarget.StandaloneWindows64, locationPathName = output, options = BuildOptions.None });
            string summary = report.summary.result + " | " + report.summary.totalErrors + " errors | " + report.summary.totalWarnings + " warnings | " + report.summary.totalSize + " bytes";
            File.WriteAllText(Path.Combine(Root, "Docs/build-result.txt"), summary);
            if (report.summary.result != BuildResult.Succeeded) throw new Exception(summary);
            Debug.Log("GARDEN_BUILD: " + summary);
        }
    }
}
