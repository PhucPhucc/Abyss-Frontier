using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion;

public class ServerConnectionUI : MonoBehaviour
{
    private enum PendingAction { None, Host, Join }

    private Button hostBtn;
    private Button joinBtn;
    private Button createBtn;
    private Button backBtn;
    private Button refreshBtn;
    private TMP_InputField serverNameInput;
    private GameObject windowHost;
    private GameObject windowServerList;
    private GameObject choosePlayerPanel;
    private TextMeshProUGUI createBtnLabel;
    private GameObject connectionPanel;
    private GameObject validationBannerRoot;
    private Text validationMessageText;
    private Button validationDismissButton;
    private Coroutine validationMessageRoutine;

    [SerializeField] private NetworkRunner runnerPrefab;

    private GameLauncher launcher;
    private PendingAction pendingAction;

    private void Awake()
    {
        FindAndCacheReferences();
        CreateValidationMessage();
        HideLegacyMenuWindows();

        if (choosePlayerPanel != null)
            choosePlayerPanel.SetActive(false);
    }

    private void Start()
    {
        EnsureLauncher();
        WireButtons();
        if (windowHost != null)
            windowHost.SetActive(false);

        if (launcher != null)
            launcher.OnRunnerStarted += OnRunnerStarted;
    }

    private void OnDestroy()
    {
        if (launcher != null)
            launcher.OnRunnerStarted -= OnRunnerStarted;
    }

    private void EnsureLauncher()
    {
        launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher == null)
        {
            Debug.LogWarning("[ServerUI] No GameLauncher found; creating one.");
            var go = new GameObject("GameLauncher");
            launcher = go.AddComponent<GameLauncher>();
            if (runnerPrefab != null)
                launcher.RunnerPrefab = runnerPrefab;
        }
    }

    private void OnRunnerStarted()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (windowHost != null) windowHost.SetActive(false);
        if (hostBtn != null) hostBtn.gameObject.SetActive(false);
        if (joinBtn != null) joinBtn.gameObject.SetActive(false);
        if (backBtn != null) backBtn.gameObject.SetActive(false);
        if (createBtn != null) createBtn.gameObject.SetActive(false);
        if (refreshBtn != null) refreshBtn.gameObject.SetActive(false);

        HideLegacyMenuWindows();

        if (choosePlayerPanel != null)
        {
            ShowChoosePlayerPanel();
            return;
        }

        Debug.LogWarning("[ServerUI] No ChoosePlayerPanel found, loading map directly.");
        if (GameSessionData.IsHost)
        {
            var launcher = FindFirstObjectByType<GameLauncher>();
            if (launcher != null)
                launcher.LoadGameScene(GameSessionData.SelectedMapScene);
        }
    }

    public void ShowLobbyAfterCharacterSelect()
    {
        HideLegacyMenuWindows();

        if (choosePlayerPanel != null)
            choosePlayerPanel.SetActive(false);

        if (GameSessionData.IsHost)
        {
            var launcher = FindFirstObjectByType<GameLauncher>();
            if (launcher != null)
            {
                Debug.Log($"[ServerUI] Host loading map: {GameSessionData.SelectedMapScene}");
                launcher.LoadGameScene(GameSessionData.SelectedMapScene);
            }
            else
            {
                Debug.LogWarning("[ServerUI] GameLauncher not found.");
            }
        }
        else
        {
            Debug.Log("[ServerUI] Client ready, waiting for host to load map...");
        }
    }

    private void FindAndCacheReferences()
    {
        var allObjs = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjs)
        {
            switch (go.name)
            {
                case "WindowHost":
                    windowHost = go;
                    break;
                case "Host":
                    if (hostBtn == null) hostBtn = go.GetComponent<Button>();
                    break;
                case "JoinBtn":
                    if (joinBtn == null) joinBtn = go.GetComponent<Button>();
                    break;
                case "CreateBtn":
                    if (createBtn == null)
                    {
                        createBtn = go.GetComponent<Button>();
                        createBtnLabel = go.GetComponentInChildren<TextMeshProUGUI>(true);
                    }
                    break;
                case "BackBtn":
                    if (backBtn == null) backBtn = go.GetComponent<Button>();
                    break;
                case "Refresh":
                    if (refreshBtn == null) refreshBtn = go.GetComponent<Button>();
                    break;
                case "ConnectionPanel":
                    if (connectionPanel == null) connectionPanel = go;
                    break;
                case "WindowServerList":
                    if (windowServerList == null) windowServerList = go;
                    break;
                case "ChoosePlayerPanel":
                    if (choosePlayerPanel == null) choosePlayerPanel = go;
                    break;
            }
        }

        serverNameInput = FindFirstObjectByType<TMP_InputField>(FindObjectsInactive.Include);
    }

    private void WireButtons()
    {
        if (hostBtn != null)
        {
            hostBtn.onClick.RemoveAllListeners();
            hostBtn.onClick.AddListener(OnHostClicked);
        }

        if (joinBtn != null)
        {
            joinBtn.onClick.RemoveAllListeners();
            joinBtn.onClick.AddListener(OnJoinClicked);
        }

        if (createBtn != null)
        {
            createBtn.onClick.RemoveAllListeners();
            createBtn.onClick.AddListener(OnCreateClicked);
        }

        if (backBtn != null)
        {
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(OnBackClicked);
        }

        if (refreshBtn != null)
        {
            refreshBtn.onClick.RemoveAllListeners();
            refreshBtn.onClick.AddListener(OnRefreshClicked);
        }
    }

    private string GetSessionName()
    {
        if (serverNameInput != null && !string.IsNullOrEmpty(serverNameInput.text))
            return serverNameInput.text;
        return GameSessionData.SessionName;
    }

    private void ShowWindowFor(PendingAction action)
    {
        pendingAction = action;
        if (windowHost != null)
        {
            if (createBtnLabel != null)
                createBtnLabel.text = action == PendingAction.Host ? "Host" : "Join";
            windowHost.SetActive(true);
        }
    }

    private void HideLegacyMenuWindows()
    {
        SetActiveIfExists(connectionPanel, false);
        SetActiveIfExists(windowHost, false);
        SetActiveIfExists(windowServerList, false);
        HideObjectsContaining("ServerList");
    }

    private static void SetActiveIfExists(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private void HideObjectsContaining(string fragment)
    {
        var allObjs = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjs)
        {
            if (go == null || go == choosePlayerPanel)
                continue;

            if (go.name.Contains(fragment))
                go.SetActive(false);
        }
    }

    private void ShowChoosePlayerPanel()
    {
        if (choosePlayerPanel == null)
            return;

        EnsureTopmostCanvas(choosePlayerPanel, 1000);

        var canvas = choosePlayerPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            var canvasChildren = canvas.transform;
            for (int i = 0; i < canvasChildren.childCount; i++)
            {
                var child = canvasChildren.GetChild(i).gameObject;
                if (child == choosePlayerPanel || child == validationBannerRoot)
                    continue;

                child.SetActive(false);
            }
        }

        choosePlayerPanel.SetActive(true);
        choosePlayerPanel.transform.SetAsLastSibling();
        Debug.Log("[ServerUI] Showing ChoosePlayerPanel");
    }

    private static void EnsureTopmostCanvas(GameObject target, int sortingOrder)
    {
        if (target == null)
            return;

        var canvas = target.GetComponent<Canvas>();
        if (canvas == null)
            canvas = target.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;

        if (target.GetComponent<GraphicRaycaster>() == null)
            target.AddComponent<GraphicRaycaster>();
    }

    private void OnHostClicked()
    {
        ShowWindowFor(PendingAction.Host);
    }

    private void OnCreateClicked()
    {
        string sessionName = GetSessionName();

        if (!GameSessionData.TryValidateSessionName(sessionName, out string validatedSessionName, out string validationError))
        {
            Debug.LogWarning($"[ServerUI] Invalid session name: {validationError}");
            ShowValidationMessage(validationError);
            return;
        }

        GameSessionData.SessionName = validatedSessionName;

        if (pendingAction == PendingAction.Host)
        {
            GameSessionData.IsMultiplayer = true;
            GameSessionData.IsHost = true;

            if (launcher != null)
                _ = launcher.LaunchAsHost(GameSessionData.SelectedMapScene, validatedSessionName);
        }
        else if (pendingAction == PendingAction.Join)
        {
            GameSessionData.IsMultiplayer = true;
            GameSessionData.IsHost = false;

            if (launcher != null)
                _ = launcher.LaunchAsClient(validatedSessionName);
        }
    }

    private void OnJoinClicked()
    {
        ShowWindowFor(PendingAction.Join);
    }

    private void OnRefreshClicked()
    {
        Debug.Log("[ServerUI] Refresh clicked");
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("Scene_Menu");
    }

    private void CreateValidationMessage()
    {
        var canvas = GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.transform : transform;

        validationBannerRoot = new GameObject("ValidationBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        validationBannerRoot.transform.SetParent(parent, false);

        var rootRect = validationBannerRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        var overlay = validationBannerRoot.GetComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.55f);
        overlay.raycastTarget = true;

        var panelObj = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObj.transform.SetParent(validationBannerRoot.transform, false);

        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(620f, 180f);
        panelRect.anchoredPosition = Vector2.zero;

        var panelImage = panelObj.GetComponent<Image>();
        panelImage.color = new Color(0.16f, 0.14f, 0.12f, 1f);

        var messageObj = new GameObject("ValidationMessage", typeof(RectTransform), typeof(Text));
        messageObj.transform.SetParent(panelObj.transform, false);

        var messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0f, 0.28f);
        messageRect.anchorMax = new Vector2(1f, 1f);
        messageRect.offsetMin = new Vector2(24f, 20f);
        messageRect.offsetMax = new Vector2(-24f, -20f);

        validationMessageText = messageObj.GetComponent<Text>();
        validationMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        validationMessageText.fontSize = 24;
        validationMessageText.alignment = TextAnchor.MiddleCenter;
        validationMessageText.color = Color.white;
        validationMessageText.text = string.Empty;

        var buttonObj = new GameObject("OkButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(panelObj.transform, false);

        var buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.sizeDelta = new Vector2(160f, 44f);
        buttonRect.anchoredPosition = new Vector2(0f, 20f);

        var buttonImage = buttonObj.GetComponent<Image>();
        buttonImage.color = new Color(0.28f, 0.48f, 0.28f, 1f);

        validationDismissButton = buttonObj.GetComponent<Button>();
        validationDismissButton.targetGraphic = buttonImage;
        validationDismissButton.onClick.AddListener(HideValidationMessageNow);

        var buttonTextObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
        buttonTextObj.transform.SetParent(buttonObj.transform, false);

        var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        var buttonText = buttonTextObj.GetComponent<Text>();
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 22;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        buttonText.text = "OK";

        validationBannerRoot.SetActive(false);
    }

    private void ShowValidationMessage(string message)
    {
        if (validationBannerRoot == null || validationMessageText == null)
            return;

        validationBannerRoot.SetActive(true);
        validationMessageText.text = message;

        if (validationMessageRoutine != null)
            StopCoroutine(validationMessageRoutine);

        validationMessageRoutine = StartCoroutine(HideValidationMessageAfterDelay());
    }

    private void HideValidationMessageNow()
    {
        if (validationMessageRoutine != null)
        {
            StopCoroutine(validationMessageRoutine);
            validationMessageRoutine = null;
        }

        if (validationBannerRoot != null)
            validationBannerRoot.SetActive(false);

        if (validationMessageText != null)
            validationMessageText.text = string.Empty;
    }

    private System.Collections.IEnumerator HideValidationMessageAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2.5f);

        HideValidationMessageNow();
        validationMessageRoutine = null;
    }
}
