using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CharacterSelectionCanvasBuilder
{
    private const string MenuScenePath = "Assets/Scenes/Scene_Menu.unity";
    private const string PrefabPath = "Assets/Prefabs/UI/SelectPlayerCanvas.prefab";
    private const string LegacyPrefabPath = "Assets/Prefabs/UI/CharacterSelectionCanvas.prefab";

    private static readonly CharacterOption[] CharacterOptions =
    {
        new CharacterOption("Hero 1", "Assets/Prefabs/Player/Hero 1.prefab"),
        new CharacterOption("Player", "Assets/Prefabs/Player/Player.prefab"),
        new CharacterOption("SPUM Hero", "Assets/Prefabs/Player/SPUM_20240911215638389 Variant.prefab"),
        new CharacterOption("Player 2", "Assets/Prefabs/Player/Player_2.prefab")
    };

    [MenuItem("Tools/UI/Rebuild Select Player Canvas")]
    public static void RebuildCharacterSelectionCanvas()
    {
        Directory.CreateDirectory("Assets/Prefabs/UI");

        string[] names = GetCharacterNames();
        Sprite[] portraits = GetCharacterPortraits();

        CharacterSelectionUI temporaryUi = CharacterSelectionUI.CreateRuntimeCanvas();
        temporaryUi.SetCharacterOptions(names, portraits);
        GameObject temporaryRoot = temporaryUi.transform.root.gameObject;
        temporaryRoot.name = "SelectPlayerCanvas";
        temporaryUi.gameObject.name = "SelectPlayerPanel";

        PrefabUtility.SaveAsPrefabAsset(temporaryUi.gameObject, PrefabPath);
        Object.DestroyImmediate(temporaryRoot);
        AssetDatabase.SaveAssets();

        Scene scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        MenuFlowController menuFlow = Object.FindFirstObjectByType<MenuFlowController>(FindObjectsInactive.Include);
        if (menuFlow == null)
        {
            Debug.LogError("Select player canvas setup failed: missing MenuFlowController.");
            return;
        }

        Transform panelParent = ResolvePanelParent(menuFlow);
        if (panelParent == null)
        {
            Debug.LogError("Select player canvas setup failed: missing menu Canvas parent.");
            return;
        }

        RemoveExistingCharacterSelectionCanvases();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        GameObject scenePanel = PrefabUtility.InstantiatePrefab(prefab, panelParent) as GameObject;
        if (scenePanel == null)
        {
            Debug.LogError($"Failed to instantiate {PrefabPath}");
            return;
        }

        scenePanel.name = "SelectPlayerPanel";

        CharacterSelectionUI sceneUi = scenePanel.GetComponent<CharacterSelectionUI>();
        if (sceneUi == null)
        {
            Debug.LogError("Select player canvas setup failed: missing CharacterSelectionUI.");
            return;
        }

        sceneUi.SetCharacterOptions(names, portraits);
        sceneUi.Configure(menuFlow);
        scenePanel.SetActive(false);

        SerializedObject flowObject = new SerializedObject(menuFlow);
        flowObject.FindProperty("characterSelectPanel").objectReferenceValue = scenePanel;
        flowObject.FindProperty("characterSelectionUI").objectReferenceValue = sceneUi;
        flowObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(scenePanel);
        EditorUtility.SetDirty(sceneUi);
        EditorUtility.SetDirty(menuFlow);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.DeleteAsset(LegacyPrefabPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Select player canvas rebuilt and wired in {MenuScenePath}");
    }

    public static void VerifySelectPlayerCanvasSetup()
    {
        VerifyRuntimeCanvasShape();
        VerifyPrefabShape();
        VerifySceneWiring();

        Debug.Log("Select player canvas verification passed.");
    }

    private static void RemoveExistingCharacterSelectionCanvases()
    {
        CharacterSelectionUI[] existingCanvases = Object.FindObjectsByType<CharacterSelectionUI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (CharacterSelectionUI selectionUI in existingCanvases)
        {
            if (selectionUI == null)
            {
                continue;
            }

            GameObject root = selectionUI.transform.root.gameObject;
            if (root.name == "CharacterSelectionCanvas"
                || root.name == "Character Selection Canvas"
                || root.name == "SelectPlayerCanvas")
            {
                Object.DestroyImmediate(root);
                continue;
            }

            GameObject selectionObject = selectionUI.gameObject;
            if (selectionObject.name == "Character Selection Panel"
                || selectionObject.name == "SelectPlayerPanel")
            {
                Object.DestroyImmediate(selectionObject);
            }
        }
    }

    private static Transform ResolvePanelParent(MenuFlowController menuFlow)
    {
        SerializedObject flowObject = new SerializedObject(menuFlow);
        GameObject chooseMapPanel = flowObject.FindProperty("chooseMapPanel").objectReferenceValue as GameObject;
        if (chooseMapPanel != null && chooseMapPanel.transform.parent != null)
        {
            return chooseMapPanel.transform.parent;
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        return canvas != null ? canvas.transform : null;
    }

    private static void VerifyRuntimeCanvasShape()
    {
        CharacterSelectionUI selectionUI = CharacterSelectionUI.CreateRuntimeCanvas();

        try
        {
            Require(selectionUI.transform.root.name == "SelectPlayerCanvas", "Runtime root must be SelectPlayerCanvas.");
            Require(selectionUI.gameObject.name == "SelectPlayerPanel", "Runtime panel must be SelectPlayerPanel.");
            Require(selectionUI.OptionCount == CharacterOptions.Length, "Runtime option count must match configured players.");
            RequireMapStyleChildren(selectionUI.transform);
        }
        finally
        {
            Object.DestroyImmediate(selectionUI.transform.root.gameObject);
        }
    }

    private static void VerifyPrefabShape()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Require(prefab != null, $"{PrefabPath} must exist.");

        CharacterSelectionUI selectionUI = prefab.GetComponent<CharacterSelectionUI>();
        Require(selectionUI != null, $"{PrefabPath} must have CharacterSelectionUI on the root.");
        RequireMapStyleChildren(prefab.transform);
    }

    private static void VerifySceneWiring()
    {
        EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);

        MenuFlowController menuFlow = Object.FindFirstObjectByType<MenuFlowController>(FindObjectsInactive.Include);
        Require(menuFlow != null, "Scene must contain MenuFlowController.");

        SerializedObject flowObject = new SerializedObject(menuFlow);
        GameObject panel = flowObject.FindProperty("characterSelectPanel").objectReferenceValue as GameObject;
        CharacterSelectionUI selectionUI = flowObject.FindProperty("characterSelectionUI").objectReferenceValue as CharacterSelectionUI;

        Require(panel != null, "MenuFlowController.characterSelectPanel must be assigned.");
        Require(selectionUI != null, "MenuFlowController.characterSelectionUI must be assigned.");
        Require(selectionUI.gameObject == panel, "CharacterSelectionUI must live on the assigned player panel.");
        Require(panel.name == "SelectPlayerPanel", "Scene player panel must be named SelectPlayerPanel.");
        Require(panel.transform.parent != null && panel.transform.parent.GetComponent<Canvas>() != null, "Scene player panel must be under the main Canvas.");
        Require(!panel.activeSelf, "Scene player panel must start inactive.");
        RequireMapStyleChildren(panel.transform);
    }

    private static void RequireMapStyleChildren(Transform root)
    {
        Require(root.Find("Window") != null, $"{root.name} must contain Window.");
        Require(root.Find("Window/SelectPlayer") != null, $"{root.name} must contain Window/SelectPlayer.");
        Transform players = root.Find("Window/Players");
        Require(players != null, $"{root.name} must contain Window/Players.");
        Require(players.childCount == CharacterOptions.Length, $"{root.name} must contain four player cards.");
        Require(root.Find("Window/BackBtn") != null, $"{root.name} must contain Window/BackBtn.");
        Require(root.Find("Window/StartBtn") != null, $"{root.name} must contain Window/StartBtn.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new System.InvalidOperationException(message);
        }
    }

    private static string[] GetCharacterNames()
    {
        string[] names = new string[CharacterOptions.Length];
        for (int i = 0; i < CharacterOptions.Length; i++)
        {
            names[i] = CharacterOptions[i].Name;
        }

        return names;
    }

    private static Sprite[] GetCharacterPortraits()
    {
        Sprite[] portraits = new Sprite[CharacterOptions.Length];
        for (int i = 0; i < CharacterOptions.Length; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterOptions[i].PrefabPath);
            SpriteRenderer renderer = prefab != null ? prefab.GetComponentInChildren<SpriteRenderer>(true) : null;
            portraits[i] = renderer != null ? renderer.sprite : null;
        }

        return portraits;
    }

    private readonly struct CharacterOption
    {
        public CharacterOption(string name, string prefabPath)
        {
            Name = name;
            PrefabPath = prefabPath;
        }

        public string Name { get; }
        public string PrefabPath { get; }
    }
}
