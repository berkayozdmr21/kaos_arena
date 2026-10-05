using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class BuildArena {
    [MenuItem("Kaos Arena/Build Windows Test")]
    public static void Build() {
        RosterAssets.Prepare();
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Kaos Arena").AddComponent<ArenaGame>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Arena.unity");
        // Runtime-generated objects still need their shader retained in the player.
        System.IO.Directory.CreateDirectory("Assets/Resources");
        if (!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/ArenaStandard.mat"))
            AssetDatabase.CreateAsset(new Material(Shader.Find("Standard")), "Assets/Resources/ArenaStandard.mat");
        if (!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/BeachSky.mat")) AssetDatabase.CreateAsset(new Material(Shader.Find("Skybox/Procedural")), "Assets/Resources/BeachSky.mat");
        AssetDatabase.SaveAssets();
        PlayerSettings.companyName = "KaosArenaPrototype";
        PlayerSettings.productName = "Kaos Arena";
        PlayerSettings.runInBackground = true;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        var report = BuildPipeline.BuildPlayer(new [] {"Assets/Scenes/Arena.unity"}, "Build/KaosArena.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);
        if (report.summary.result != BuildResult.Succeeded) throw new System.Exception("Windows build failed: " + report.summary.result);
    }
}
