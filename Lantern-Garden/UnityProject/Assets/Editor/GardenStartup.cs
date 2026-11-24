using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LanternGarden.Editor
{
    [InitializeOnLoad]
    public static class GardenStartup
    {
        static GardenStartup() { EditorApplication.delayCall += Initialize; }

        static void Initialize()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            ConfigureRenderingAndIcon();
            if (!File.Exists(GardenBuilder.ScenePath)) return;
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(GardenBuilder.ScenePath);
            var scene = SceneManager.GetActiveScene();
            // Replace only Unity's untouched default scene; never discard a user's work.
            if (SceneManager.sceneCount == 1 && scene.IsValid() && string.IsNullOrEmpty(scene.path) && !scene.isDirty &&
                scene.GetRootGameObjects().All(o => o.name == "Main Camera" || o.name == "Directional Light"))
                EditorSceneManager.OpenScene(GardenBuilder.ScenePath);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Lantern Garden/Open garden scene")]
        public static void OpenGarden()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(GardenBuilder.ScenePath);
        }

        static object[] BatchingSettings()
        {
            // Unity 6.5 marks the public dynamic-batching setter obsolete with a compile error.
            // Its underlying settings API still allows existing projects to turn the feature off.
            object[] values = { BuildTarget.StandaloneWindows64, 0, 0 };
            var getter = typeof(PlayerSettings).GetMethod("GetBatchingForPlatform", BindingFlags.Static | BindingFlags.NonPublic);
            if (getter == null) throw new NotSupportedException("Unity batching settings API has changed.");
            getter.Invoke(null, values);
            return values;
        }

        public static bool DynamicBatchingDisabled() { return (int)BatchingSettings()[2] == 0; }

        public static void ConfigureRenderingAndIcon()
        {
            var batching = BatchingSettings();
            if ((int)batching[2] != 0)
            {
                var setter = typeof(PlayerSettings).GetMethod("SetBatchingForPlatform", BindingFlags.Static | BindingFlags.NonPublic);
                if (setter == null) throw new NotSupportedException("Unity batching settings API has changed.");
                batching[2] = 0; // Preserve the existing static-batching setting.
                setter.Invoke(null, batching);
            }
            foreach (string id in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(id));
                if (!material.enableInstancing) { material.enableInstancing = true; EditorUtility.SetDirty(material); }
            }
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Branding/lantern-garden-icon.png");
            if (icon) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
        }
    }
}
