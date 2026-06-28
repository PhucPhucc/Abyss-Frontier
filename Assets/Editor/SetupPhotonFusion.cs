using UnityEngine;
using UnityEditor;
using Fusion;

public static class SetupPhotonFusion
{
    [MenuItem("Tools/Fusion/Full Setup (Run Once)")]
    public static void FullSetup()
    {
        CreateRunnerPrefab();
        SetupPlayerPrefab();
        AddScenesToBuildSettings();
        Debug.Log("=== Fusion Setup Complete! ===");
        Debug.Log("Next: Assign references in the Unity Editor (see guide).");
    }

    [MenuItem("Tools/Fusion/Create Runner Prefab")]
    public static void CreateRunnerPrefab()
    {
        GameObject go = new GameObject("NetworkRunner");

        go.AddComponent<NetworkRunner>();
        go.AddComponent<NetworkSceneManagerDefault>();
        go.AddComponent<PlayerSpawner>();
        go.AddComponent<InputHandler>();

        string path = "Assets/Prefabs/NetworkRunner.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        Debug.Log($"Runner prefab created at: {path}");
    }

    [MenuItem("Tools/Fusion/Setup Player Prefab")]
    public static void SetupPlayerPrefab()
    {
        string path = "Assets/Prefabs/Player/Player.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogError($"Player prefab not found at {path}"); return; }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null) return;

        NetworkObject netObj = instance.GetComponent<NetworkObject>();
        if (netObj == null) netObj = instance.AddComponent<NetworkObject>();

        NetworkTransform netTransform = instance.GetComponent<NetworkTransform>();
        if (netTransform == null)
        {
            instance.AddComponent<NetworkTransform>();
        }

        NetworkPlayer netPlayer = instance.GetComponent<NetworkPlayer>();
        if (netPlayer == null) instance.AddComponent<NetworkPlayer>();

        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);

        AssetDatabase.SetLabels(prefab, new[] { "FusionPrefab" });

        Debug.Log($"Player prefab updated with Network components and FusionPrefab label");
    }

    [MenuItem("Tools/Fusion/Add Scenes to Build Settings")]
    public static void AddScenesToBuildSettings()
    {
        string[] scenePaths = {
            "Assets/Scenes/floor_1.unity",
            "Assets/Scenes/floor_2.unity",
            "Assets/Scenes/floor_3.unity",
            "Assets/Scenes/floor_4.unity",
            "Assets/Scenes/floor_5.unity"
        };

        var scenes = new EditorBuildSettingsScene[scenePaths.Length];
        for (int i = 0; i < scenePaths.Length; i++)
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePaths[i]);
            if (scene != null)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }
        }

        EditorBuildSettings.scenes = scenes;
        Debug.Log($"Added {scenePaths.Length} scenes to Build Settings");
    }
}
