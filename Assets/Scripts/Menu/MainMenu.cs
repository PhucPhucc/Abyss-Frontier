using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject ContinueButton;
    [SerializeField] private GameObject LevelSelectPanel;
    [SerializeField] private GameObject ConfirmPanel;
    [SerializeField] private SaveConfirmPopup saveConfirmPopup;

    private void Start()
    {
        GameSessionData.ResetSession();

        if (ContinueButton != null)
            ContinueButton.SetActive(false);

        if (CloudServiceManager.Instance != null)
            CloudServiceManager.Instance.AuthReady += OnAuthReady;
        else
            UpdateContinueButton();
    }

    private void OnDestroy()
    {
        if (CloudServiceManager.Instance != null)
            CloudServiceManager.Instance.AuthReady -= OnAuthReady;
    }

    private void OnAuthReady()
    {
        UpdateContinueButton();
    }

    private void UpdateContinueButton()
    {
        if (ContinueButton != null)
            ContinueButton.SetActive(SaveManager.Instance != null && SaveManager.Instance.HasSavedGame);
    }

    public void PlayGame()
    {
        string target = string.IsNullOrEmpty(GameSessionData.SelectedMapScene) ? "floor1" : GameSessionData.SelectedMapScene;

        if (SaveManager.HasSaveForMap(target))
        {
            ShowOverwriteWarning(target);
            return;
        }

        StartGameplay(target);
    }

    private void StartGameplay(string target)
    {
        if (GameSessionData.SelectedCharacterPrefab != null)
        {
            Debug.Log($"[MainMenu] Using MenuFlow selection: map={target}");
            var flow = FindFirstObjectByType<MenuFlowController>();
            if (flow != null)
            {
                flow.StartGame();
                return;
            }
        }

        Debug.Log($"[MainMenu] Loading {target}");
        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
            _ = launcher.LaunchAsSingleplayer(target);
        else
            SceneManager.LoadScene(target);
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
        text.text = $"Overwrite saved data?\n{FormatMapName(sceneName)} has existing save.";
        text.fontSize = 26;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var playBtn = CreatePopupButton(panelObj.transform, "PlayBtn", "Play", new Color(0.2f, 0.5f, 0.2f), () =>
        {
            Destroy(canvasObj);
            SaveManager.ClearSaveForMap(sceneName);
            StartGameplay(sceneName);
        });

        var backBtn = CreatePopupButton(panelObj.transform, "BackBtn", "Back", new Color(0.5f, 0.2f, 0.2f), () =>
        {
            Destroy(canvasObj);
        });

        var btnRectP = playBtn.GetComponent<RectTransform>();
        btnRectP.anchorMin = new Vector2(0.25f, 0f);
        btnRectP.anchorMax = new Vector2(0.25f, 0f);
        btnRectP.sizeDelta = new Vector2(80, 36);
        btnRectP.anchoredPosition = new Vector2(-50, 28);

        var btnRectB = backBtn.GetComponent<RectTransform>();
        btnRectB.anchorMin = new Vector2(0.75f, 0f);
        btnRectB.anchorMax = new Vector2(0.75f, 0f);
        btnRectB.sizeDelta = new Vector2(80, 36);
        btnRectB.anchoredPosition = new Vector2(50, 28);
    }

    private static Button CreatePopupButton(Transform parent, string name, string label, Color color, System.Action onClick)
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

    public void ContinueFromSave()
    {
        if (SaveManager.Instance == null) return;

        string selected = GameSessionData.SelectedMapScene;
        var savedMaps = SaveManager.GetSavedMaps();

        if (!string.IsNullOrEmpty(selected))
        {
            if (SaveManager.HasSaveForMap(selected))
            {
                SaveManager.Instance.ContinueGame(selected);
                return;
            }

            ShowNoSavePopup(selected);
            return;
        }

        if (savedMaps == null || savedMaps.Count == 0)
        {
            ShowNoSavePopup("floor1");
            return;
        }

        SaveManager.Instance.ContinueGame(savedMaps[0]);
    }

    private void ShowNoSavePopup(string sceneName)
    {
        var canvasObj = new GameObject("NoSavePopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

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
        text.text = $"No save - {FormatMapName(sceneName)}";
        text.fontSize = 28;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var okBtnObj = new GameObject("OK", typeof(Image), typeof(Button));
        okBtnObj.transform.SetParent(panelObj.transform, false);
        var okBtnRect = okBtnObj.GetComponent<RectTransform>();
        okBtnRect.anchorMin = new Vector2(0.5f, 0f);
        okBtnRect.anchorMax = new Vector2(0.5f, 0f);
        okBtnRect.sizeDelta = new Vector2(80, 36);
        okBtnRect.anchoredPosition = new Vector2(0, 28);
        var okImage = okBtnObj.GetComponent<Image>();
        okImage.color = new Color(0.3f, 0.3f, 0.5f, 1);

        var okBtn = okBtnObj.GetComponent<Button>();
        okBtn.targetGraphic = okImage;
        okBtn.onClick.AddListener(() => Destroy(canvasObj));

        var okLabelObj = new GameObject("Label", typeof(Text));
        okLabelObj.transform.SetParent(okBtnObj.transform, false);
        var okLabelRect = okLabelObj.GetComponent<RectTransform>();
        okLabelRect.anchorMin = Vector2.zero;
        okLabelRect.anchorMax = Vector2.one;
        okLabelRect.sizeDelta = Vector2.zero;
        var okLabel = okLabelObj.GetComponent<Text>();
        okLabel.text = "OK";
        okLabel.fontSize = 24;
        okLabel.alignment = TextAnchor.MiddleCenter;
        okLabel.color = Color.white;
        okLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static string FormatMapName(string sceneName)
    {
        return sceneName.Replace("floor", "Floor ");
    }

    public void NewGame()
    {
        ClearSave();
        EnemyHealth.KilledEnemyIds.Clear();
        SaveManager.UnlockedFloors.Clear();
        SaveManager.UnlockedFloors.AddRange(new[] { "floor1", "floor2", "floor3", "floor4", "floor5" });
        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
            _ = launcher.LaunchAsSingleplayer("floor1");
        else
            SceneManager.LoadScene("floor1");
    }

    public void OpenLevelSelect()
    {
        if (LevelSelectPanel != null)
            LevelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        if (LevelSelectPanel != null)
            LevelSelectPanel.SetActive(false);
    }

    public void ExitGame()
    {
        if (ConfirmPanel != null)
        {
            ConfirmPanel.SetActive(true);
            return;
        }

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ConfirmExitByYes()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ConfirmExitByNo()
    {
        if (ConfirmPanel != null)
            ConfirmPanel.SetActive(false);
    }

    private void ClearSave()
    {
        if (SaveManager.Instance != null)
            SaveManager.ClearSavedDataFlag();
    }

    public void OpenSettings()
    {
        if (SettingsPanel != null)
            SettingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);
    }
}