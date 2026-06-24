using UnityEngine;
using UnityEditor;

public class FixEnemyPrefabs
{
    [MenuItem("Tools/Fix Enemy Prefabs")]
    public static void FixAll()
    {
        FixOrcPrefab("Assets/Prefabs/Orc/Orc1.prefab");
        FixOrcPrefab("Assets/Prefabs/Orc/Orc2.prefab");
        FixOrcPrefab("Assets/Prefabs/Orc/Orc3.prefab");
        FixPlayerPrefab("Assets/Prefabs/Player.prefab");
        Debug.Log("Done fixing prefabs!");
    }

    private static void FixOrcPrefab(string path)
    {
        GameObject go = PrefabUtility.LoadPrefabContents(path);
        if (go == null) { Debug.LogError($"Cannot load: {path}"); return; }

        bool changed = false;

        if (go.GetComponent<CapsuleCollider2D>() == null)
        {
            CapsuleCollider2D col = go.AddComponent<CapsuleCollider2D>();
            col.offset = Vector2.zero;
            col.size = new Vector2(0.6f, 0.8f);
            changed = true;
            Debug.Log($"Added CapsuleCollider2D to {path}");
        }

        EnemyAI[] aiComponents = go.GetComponents<EnemyAI>();
        if (aiComponents.Length > 1)
        {
            for (int i = 1; i < aiComponents.Length; i++)
            {
                Object.DestroyImmediate(aiComponents[i], true);
                changed = true;
            }
            Debug.Log($"Removed duplicate EnemyAI from {path}");
        }

        if (changed)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Debug.Log($"Saved {path}");
        }

        PrefabUtility.UnloadPrefabContents(go);
    }

    private static void FixPlayerPrefab(string path)
    {
        GameObject go = PrefabUtility.LoadPrefabContents(path);
        if (go == null) { Debug.LogError($"Cannot load: {path}"); return; }

        bool changed = false;

        if (go.GetComponent<PlayerStats>() == null)
        {
            go.AddComponent<PlayerStats>();
            changed = true;
            Debug.Log("Added PlayerStats to Player");
        }

        if (go.GetComponent<PlayerHealth>() == null)
        {
            go.AddComponent<PlayerHealth>();
            changed = true;
            Debug.Log("Added PlayerHealth to Player");
        }

        if (changed)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Debug.Log("Saved Player prefab");
        }

        PrefabUtility.UnloadPrefabContents(go);
    }
}
