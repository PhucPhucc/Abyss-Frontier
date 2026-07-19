using UnityEditor;
using UnityEngine;
using Fusion;
using System.IO;

public class AttachNetworkEnemyEditor : EditorWindow
{
    [MenuItem("Tools/Abyss Frontier/Attach NetworkEnemy to Prefabs")]
    public static void AttachComponents()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs", "Assets/SPUM" });
        int updatedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            // Check if the prefab has EnemyHealth but lacks NetworkEnemy
            if (prefab.GetComponent<EnemyHealth>() != null)
            {
                if (prefab.GetComponent<NetworkEnemy>() == null)
                {
                    // Open prefab stage or load prefab to edit
                    GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);

                    // Add NetworkEnemy (this will automatically add NetworkObject due to RequireComponent attribute)
                    if (prefabInstance.GetComponent<NetworkEnemy>() == null)
                    {
                        prefabInstance.AddComponent<NetworkEnemy>();
                        PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                        updatedCount++;
                        Debug.Log($"[NetworkEnemy Attacher] Successfully attached NetworkEnemy to: {path}");
                    }

                    PrefabUtility.UnloadPrefabContents(prefabInstance);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Attach NetworkEnemy Component",
            $"Completed scanning. Attached NetworkEnemy (and NetworkObject) to {updatedCount} enemy prefabs.",
            "OK"
        );
    }
}
