using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class MenuFlowController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject chooseMapPanel;
    [SerializeField] private GameObject playModePanel;
    [SerializeField] private GameObject hostJoinPanel;
    [SerializeField] private GameObject characterSelectPanel;

    [Header("Navigation Buttons (Optional)")]
    [SerializeField] private Button mapNextButton;
    [SerializeField] private Button playModeNextButton;
    [SerializeField] private Button startMatchButton;

    private void Start()
    {
        if (GameSessionData.OpenMapPanelNext)
        {
            GameSessionData.OpenMapPanelNext = false;
            ShowOnlyPanel(chooseMapPanel);
        }
        else
        {
            ShowOnlyPanel(mainMenuPanel);
        }

        if (mapNextButton != null) mapNextButton.interactable = false;
        if (playModeNextButton != null) playModeNextButton.interactable = false;
        if (startMatchButton != null) startMatchButton.interactable = false;

        if (hostJoinPanel != null)
            hostJoinPanel.SetActive(false);
    }

    private void ShowOnlyPanel(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(mainMenuPanel == activePanel);
        if (chooseMapPanel != null) chooseMapPanel.SetActive(chooseMapPanel == activePanel);
        if (playModePanel != null) playModePanel.SetActive(playModePanel == activePanel);
        if (hostJoinPanel != null) hostJoinPanel.SetActive(hostJoinPanel == activePanel);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(characterSelectPanel == activePanel);
    }

    #region Step 1: Main Menu -> Choose Map
    public void OnPlayButtonClicked()
    {
        ShowOnlyPanel(chooseMapPanel);
    }
    #endregion

    #region Step 2: Choose Map -> Play Mode
    public void SelectMapIndex(int mapIndex)
    {
        GameSessionData.SelectedMapScene = $"floor{mapIndex}";
        Debug.Log($"[MenuFlow] Map: {GameSessionData.SelectedMapScene}");

        if (mapNextButton != null) mapNextButton.interactable = true;
    }

    public void SelectMapSceneName(string sceneName)
    {
        GameSessionData.SelectedMapScene = sceneName;
        Debug.Log($"[MenuFlow] Map: {GameSessionData.SelectedMapScene}");

        if (mapNextButton != null) mapNextButton.interactable = true;
    }

    public void OnMapSelectionNextClicked()
    {
        ShowOnlyPanel(playModePanel);
    }

    public void BackToMainMenu()
    {
        ShowOnlyPanel(mainMenuPanel);
    }
    #endregion

    #region Step 3: Play Mode -> Host/Join or Character Select
    public void SelectPlayModeInt(int modeIndex)
    {
        if (modeIndex == 1)
        {
            GameSessionData.IsMultiplayer = true;
            LoadServerScene();
        }
        else
        {
            GameSessionData.IsMultiplayer = false;
            OnPlayModeNextClicked();
        }
    }

    public void SelectPlayMode(bool isMultiplayer)
    {
        if (isMultiplayer)
        {
            GameSessionData.IsMultiplayer = true;
            LoadServerScene();
        }
        else
        {
            GameSessionData.IsMultiplayer = false;
            OnPlayModeNextClicked();
        }
    }

    private void LoadServerScene()
    {
        SceneManager.LoadScene("Scene-Server");
    }

    private void ShowHostJoinPanel()
    {
        if (hostJoinPanel != null)
        {
            var hostJoin = hostJoinPanel.GetComponent<HostJoinUI>();
            if (hostJoin != null) hostJoin.Show();
            ShowOnlyPanel(hostJoinPanel);
        }
        else
        {
            Debug.LogWarning("[MenuFlow] No HostJoinPanel assigned! Going to CharacterSelect directly.");
            OnPlayModeNextClicked();
        }
    }

    public void OnHostSelected()
    {
        OnPlayModeNextClicked();
    }

    public void OnJoinSelected()
    {
        OnPlayModeNextClicked();
    }

    public void OnHostJoinBack()
    {
        GameSessionData.IsMultiplayer = false;
        ShowOnlyPanel(playModePanel);
    }

    public void OnPlayModeNextClicked()
    {
        if (characterSelectPanel != null)
        {
            ShowOnlyPanel(characterSelectPanel);
        }
        else
        {
            Debug.LogWarning("[MenuFlow] CharacterSelectPanel not assigned! Starting game...");
            StartGame();
        }
    }

    public void BackToChooseMap()
    {
        ShowOnlyPanel(chooseMapPanel);
    }
    #endregion

    #region Step 4: Character Select -> Start Game
    public void SelectCharacterIndex(int charIndex)
    {
        SelectCharacter(charIndex, null);
    }

    public void SelectCharacter(int charIndex, CharacterData characterData)
    {
        GameSessionData.SelectCharacter(charIndex, characterData);
        string prefabName = GameSessionData.SelectedCharacterPrefab != null
            ? GameSessionData.SelectedCharacterPrefab.name
            : "None";
        Debug.Log($"[MenuFlow] Character index: {charIndex} | Prefab: {prefabName}");

        if (startMatchButton != null) startMatchButton.interactable = true;
    }

    public void BackToPlayMode()
    {
        ShowOnlyPanel(playModePanel);
    }
    public void StartGame()
    {
        string scene = GameSessionData.SelectedMapScene;

        if (SaveManager.HasSaveForMap(scene))
        {
            ShowOverwriteWarning(scene);
            return;
        }

        DoStartGame(scene);
    }

    private void DoStartGame(string scene)
    {
        string prefabName = GameSessionData.SelectedCharacterPrefab != null
            ? GameSessionData.SelectedCharacterPrefab.name
            : "None";
        Debug.Log($"[MenuFlow] Scene: {scene} | Multiplayer: {GameSessionData.IsMultiplayer} | CharIndex: {GameSessionData.SelectedCharacterIndex} | Prefab: {prefabName}");

        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher == null)
        {
            Debug.LogError("[MenuFlow] No GameLauncher found!");
            return;
        }

        if (GameSessionData.IsMultiplayer)
        {
            if (GameSessionData.IsHost)
                _ = launcher.LaunchAsHost(scene, GameSessionData.SessionName);
            else
                _ = launcher.LaunchAsClient(GameSessionData.SessionName);
        }
        else
        {
            _ = launcher.LaunchAsSingleplayer(scene);
        }
    }

    private void ShowOverwriteWarning(string sceneName)
    {
        var canvasObj = new GameObject("OverwriteWarning", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var overlay = new GameObject("Overlay", typeof(Image));
        overlay.transform.SetParent(canvasObj.transform, false);
        var overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        var overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.6f);

        var panelObj = new GameObject("Panel", typeof(Image));
        panelObj.transform.SetParent(canvasObj.transform, false);
        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 160);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        var panelImage = panelObj.GetComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.15f, 1);

        var textObj = new GameObject("Message", typeof(Text));
        textObj.transform.SetParent(panelObj.transform, false);
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 50);
        textRect.offsetMax = new Vector2(-20, -10);
        var text = textObj.GetComponent<Text>();
        text.text = $"Overwrite saved data?\n{sceneName.Replace("floor", "Floor ")} has existing save.";
        text.fontSize = 26;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var playBtn = CreateBtn(panelObj.transform, "PlayBtn", "Play", new Color(0.2f, 0.5f, 0.2f), () =>
        {
            Destroy(canvasObj);
            SaveManager.ClearSaveForMap(sceneName);
            DoStartGame(sceneName);
        });

        var backBtn = CreateBtn(panelObj.transform, "BackBtn", "Back", new Color(0.5f, 0.2f, 0.2f), () =>
        {
            Destroy(canvasObj);
        });

        var rp = playBtn.GetComponent<RectTransform>();
        rp.anchorMin = new Vector2(0.25f, 0f);
        rp.anchorMax = new Vector2(0.25f, 0f);
        rp.sizeDelta = new Vector2(80, 36);
        rp.anchoredPosition = new Vector2(-50, 28);

        var rb = backBtn.GetComponent<RectTransform>();
        rb.anchorMin = new Vector2(0.75f, 0f);
        rb.anchorMax = new Vector2(0.75f, 0f);
        rb.sizeDelta = new Vector2(80, 36);
        rb.anchoredPosition = new Vector2(50, 28);
    }

    private static Button CreateBtn(Transform parent, string name, string label, Color color, Action onClick)
    {
        var btnObj = new GameObject(name, typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        var image = btnObj.GetComponent<Image>();
        image.color = color;

        var btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = image;
        btn.onClick.AddListener(() => onClick());

        var labelObj = new GameObject("Label", typeof(Text));
        labelObj.transform.SetParent(btnObj.transform, false);
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        var labelText = labelObj.GetComponent<Text>();
        labelText.text = label;
        labelText.fontSize = 24;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return btn;
    }
    #endregion
}
