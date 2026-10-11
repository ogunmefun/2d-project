using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonRelay.Editor
{
    [InitializeOnLoad]
    public static class BuildTools
    {
        public const string MainScene = "Assets/Scenes/NeonRelay.unity";
        static readonly string ArtifactDir = Path.GetFullPath(Path.Combine(Application.dataPath,"../../artifacts"));
        static double nextPoll;
        static DateTime lastCodeWrite;
        static BuildTools() { EditorApplication.delayCall += SetupStartScene; EditorApplication.update += PollRequest; }

        static void SetupStartScene()
        {
            EnsureScene();
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScene);
        }
        [MenuItem("Neon Relay/Play game")]
        public static void PlayGame() { SetupStartScene(); EditorApplication.isPlaying=true; }

        static void EnsureScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(MainScene)) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag="MainCamera"; camera.orthographic=true; camera.orthographicSize=8;
            camera.transform.position=new Vector3(16,7,-10);
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.035f,.055f,.1f);
            camera.gameObject.AddComponent<AudioListener>();
            new GameObject("Neon Relay - Game").AddComponent<NeonRelayGame>();
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene,MainScene);
            Configure();
            AssetDatabase.SaveAssets();
        }

        static void Configure()
        {
            EditorBuildSettings.scenes=new[] { new EditorBuildSettingsScene(MainScene,true) };
            PlayerSettings.companyName="Ogunmefun"; PlayerSettings.productName="Neon Relay";
            PlayerSettings.bundleVersion="1.2.0";
            PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=720;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=true;
            PlayerSettings.runInBackground=false;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.Standalone,ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.colorSpace=ColorSpace.Linear;
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback=false;
            PlayerSettings.WebGL.dataCaching=false;
            PlayerSettings.WebGL.template="APPLICATION:Minimal";
            EditorSettings.defaultBehaviorMode=EditorBehaviorMode.Mode2D;
        }

        [MenuItem("Neon Relay/Run gameplay checks")]
        public static void Verify()
        {
            Directory.CreateDirectory(ArtifactDir);
            try { var result=GameChecks.Run(); File.WriteAllText(Path.Combine(ArtifactDir,"gameplay-checks.txt"),result); Debug.Log(result); }
            catch(Exception e) { File.WriteAllText(Path.Combine(ArtifactDir,"gameplay-checks.txt"),e.ToString()); Debug.LogException(e); throw; }
        }

        [MenuItem("Neon Relay/Build Windows (with checks)")]
        public static void VerifyAndBuild()
        {
            EnsureScene(); Configure(); Verify();
            Build("Builds/Windows/NeonRelay.exe",BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Neon Relay/Build WebGL (with checks)")]
        public static void BuildWeb()
        {
            EnsureScene(); Configure(); Verify();
            Build("Builds/WebGL",BuildTarget.WebGL);
        }

        static void Build(string path,BuildTarget target)
        {
            AssetDatabase.SaveAssets();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[] {MainScene},locationPathName=path,target=target,options=BuildOptions.None });
            string result=report.summary.result+" | "+report.summary.totalErrors+" errors | "+report.summary.totalWarnings+" warnings | "+report.summary.totalSize+" bytes";
            File.WriteAllText(Path.Combine(ArtifactDir,"build-"+target+".txt"),result);
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Build failed: "+result);
            Debug.Log("Neon Relay build: "+result);
        }

        // Optional local automation: Tools/request.ps1 writes this file. No network or gameplay hooks.
        static void PollRequest()
        {
            if(EditorApplication.timeSinceStartup<nextPoll || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            nextPoll=EditorApplication.timeSinceStartup+2;
            // Refresh source edits even when the editor is behind another application.
            DateTime newest=DateTime.MinValue;
            foreach(string source in Directory.GetFiles(Application.dataPath,"*.cs",SearchOption.AllDirectories))
                if(File.GetLastWriteTimeUtc(source)>newest) newest=File.GetLastWriteTimeUtc(source);
            if(newest!=lastCodeWrite) { lastCodeWrite=newest; AssetDatabase.Refresh(); return; }
            string path=Path.Combine(ArtifactDir,"editor-request.txt");
            if(!File.Exists(path)) return;
            string request=File.ReadAllText(path).Trim(); File.Delete(path);
            try
            {
                if(request=="verify") Verify();
                else if(request=="windows") VerifyAndBuild();
                else if(request=="web") BuildWeb();
                else if(request=="open") EditorSceneManager.OpenScene(MainScene);
                else throw new Exception("Unknown editor request: "+request);
                File.WriteAllText(Path.Combine(ArtifactDir,"request-result.txt"),"SUCCESS: "+request);
            }
            catch(Exception e) { File.WriteAllText(Path.Combine(ArtifactDir,"request-result.txt"),"FAILED: "+e); }
        }
    }
}
