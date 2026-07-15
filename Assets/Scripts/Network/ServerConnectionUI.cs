using System.Collections;
using System.Collections.Generic;
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
    private GameObject windowJoin;
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

    // Session list
    private Transform sessionListContent;
    private List<SessionInfo> cachedSessions = new();
    private TextMeshProUGUI sessionListEmptyText;
    private Coroutine refreshIndicatorCoroutine;

    // Disconnect / error popup
    private GameObject disconnectOverlay;

    private void Awake()
    {
        FindAndCacheReferences();
        CreateJoinWindow();
        CreateValidationMessage();
        CreateDisconnectOverlay();
        HideLegacyMenuWindows();

        if (choosePlayerPanel != null)
            choosePlayerPanel.SetActive(false);
    }

    private void Start()
    {
        GameSessionData.ResetSession();

        EnsureLauncher();
        WireButtons();
        if (windowHost != null)
            windowHost.SetActive(false);

        if (launcher != null)
        {
            launcher.OnRunnerStarted += OnRunnerStarted;
            launcher.SessionListUpdated += OnSessionListUpdated;
            launcher.Disconnected += OnDisconnected;
            launcher.ConnectFailed += OnConnectError;
        }
    }

    private void OnDestroy()
    {
        if (launcher != null)
        {
            launcher.OnRunnerStarted -= OnRunnerStarted;
            launcher.SessionListUpdated -= OnSessionListUpdated;
            launcher.Disconnected -= OnDisconnected;
            launcher.ConnectFailed -= OnConnectError;
        }
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
        if (windowServerList != null) windowServerList.SetActive(false);

        HideLegacyMenuWindows();

        if (choosePlayerPanel != null)
        {
            ShowChoosePlayerPanel();
            return;
        }

        Debug.LogWarning("[ServerUI] No ChoosePlayerPanel found, loading map directly.");
        if (GameSessionData.IsHost)
        {
            var l = FindFirstObjectByType<GameLauncher>();
            if (l != null)
                l.LoadGameScene(GameSessionData.SelectedMapScene);
        }
    }

    public void ShowLobbyAfterCharacterSelect()
    {
        HideLegacyMenuWindows();

        if (choosePlayerPanel != null)
            choosePlayerPanel.SetActive(false);

        var lobbyUi = FindFirstObjectByType<LobbyUI>();
        if (lobbyUi != null)
        {
            Debug.Log($"[ServerUI] Showing lobby UI (isHost={GameSessionData.IsHost})");
            lobbyUi.Show(GameSessionData.IsHost);
        }
        else
        {
            Debug.LogWarning("[ServerUI] LobbyUI not found in scene.");
            if (GameSessionData.IsHost)
            {
                var l = FindFirstObjectByType<GameLauncher>();
                if (l != null)
                {
                    Debug.Log($"[ServerUI] Fallback: Host loading map directly: {GameSessionData.SelectedMapScene}");
                    l.LoadGameScene(GameSessionData.SelectedMapScene);
                }
            }
            else
            {
                Debug.Log("[ServerUI] Client ready, waiting for host to load map...");
            }
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

    private void CreateJoinWindow()
    {
        var canvas = GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.transform : transform;

        windowJoin = new GameObject("WindowJoin", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        windowJoin.transform.SetParent(parent, false);

        var rootRect = windowJoin.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(500, 300);
        rootRect.anchoredPosition = Vector2.zero;

        windowJoin.GetComponent<Image>().color = new Color(0.12f, 0.10f, 0.08f, 0.97f);

        // Title
        CreateTMP(windowJoin.transform, "Title", "Tham gia phòng",
            new Vector2(0, 100), new Vector2(440, 50), 28, TextAlignmentOptions.Center);

        // Session name input
        var inputObj = new GameObject("SessionNameInput", typeof(Image), typeof(TMP_InputField), typeof(RectTransform));
        inputObj.transform.SetParent(windowJoin.transform, false);
        var inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.sizeDelta = new Vector2(380, 50);
        inputRect.anchoredPosition = new Vector2(0, 20);
        inputObj.GetComponent<Image>().color = new Color(0.2f, 0.18f, 0.15f, 1f);

        var placeholder = new GameObject("Placeholder", typeof(RectTransform));
        placeholder.transform.SetParent(inputObj.transform, false);
        var phRect = placeholder.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = new Vector2(16, 0);
        phRect.offsetMax = new Vector2(-16, 0);
        var phTmp = placeholder.AddComponent<TextMeshProUGUI>();
        phTmp.text = "Nhập tên phòng...";
        phTmp.fontSize = 18;
        phTmp.fontStyle = FontStyles.Italic;
        phTmp.color = new Color(1f, 1f, 1f, 0.4f);

        var textArea = new GameObject("Text Area", typeof(RectTransform));
        textArea.transform.SetParent(inputObj.transform, false);
        var taRect = textArea.GetComponent<RectTransform>();
        taRect.anchorMin = Vector2.zero;
        taRect.anchorMax = Vector2.one;
        taRect.offsetMin = new Vector2(16, 0);
        taRect.offsetMax = new Vector2(-16, 0);

        var inputText = new GameObject("Text", typeof(RectTransform));
        inputText.transform.SetParent(textArea.transform, false);
        var itRect = inputText.GetComponent<RectTransform>();
        itRect.anchorMin = Vector2.zero;
        itRect.anchorMax = Vector2.one;
        itRect.offsetMin = Vector2.zero;
        itRect.offsetMax = Vector2.zero;
        var itTmp = inputText.AddComponent<TextMeshProUGUI>();
        itTmp.fontSize = 18;
        itTmp.color = Color.white;

        var inputField = inputObj.GetComponent<TMP_InputField>();
        inputField.textViewport = textArea.GetComponent<RectTransform>();
        inputField.textComponent = itTmp;
        inputField.placeholder = phTmp;

        // Join button
        var joinBtnObj = CreateButton(windowJoin.transform, "JoinBtn", "Tham gia",
            new Vector2(0, -40), new Vector2(200, 50), new Color(0.25f, 0.55f, 0.25f, 1f));
        joinBtnObj.onClick.AddListener(() =>
        {
            string sessionName = inputField.text;
            if (string.IsNullOrEmpty(sessionName))
                sessionName = GameSessionData.SessionName;

            if (!GameSessionData.TryValidateSessionName(sessionName, out string validated, out string error))
            {
                ShowValidationMessage(error);
                return;
            }

            GameSessionData.SessionName = validated;
            GameSessionData.IsMultiplayer = true;
            GameSessionData.IsHost = false;

            windowJoin.SetActive(false);
            if (launcher != null)
                _ = launcher.LaunchAsClient(validated);
        });

        // Browse button
        var browseBtnObj = CreateButton(windowJoin.transform, "BrowseBtn", "Duyệt phòng",
            new Vector2(0, -100), new Vector2(200, 44), new Color(0.3f, 0.4f, 0.6f, 1f));
        browseBtnObj.onClick.AddListener(() =>
        {
            windowJoin.SetActive(false);
            ShowServerListWindow();
        });

        // Back button
        var backBtnObj = CreateButton(windowJoin.transform, "BackBtn", "Quay lại",
            new Vector2(0, -155), new Vector2(140, 40), new Color(0.4f, 0.2f, 0.2f, 1f));
        backBtnObj.onClick.AddListener(() => windowJoin.SetActive(false));

        windowJoin.SetActive(false);
    }

    private void HideLegacyMenuWindows()
    {
        SetActiveIfExists(connectionPanel, false);
        SetActiveIfExists(windowHost, false);
        SetActiveIfExists(windowJoin, false);
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
                if (child == choosePlayerPanel || child == validationBannerRoot || child == disconnectOverlay)
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

    // ── Button Handlers ──

    private void OnHostClicked()
    {
        pendingAction = PendingAction.Host;
        if (windowHost != null)
            windowHost.SetActive(true);
    }

    private void OnJoinClicked()
    {
        if (windowJoin != null)
        {
            pendingAction = PendingAction.Join;
            windowJoin.SetActive(true);
        }
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

    private void OnRefreshClicked()
    {
        Debug.Log("[ServerUI] Refresh clicked — joining session lobby...");
        if (launcher != null)
            launcher.JoinSessionLobby();
    }

    private void OnBackClicked()
    {
        if (launcher != null)
            launcher.ShutdownRunner();
        SceneManager.LoadScene("Scene_Menu");
    }

    // ── Session List Window ──

    private void ShowServerListWindow()
    {
        pendingAction = PendingAction.Join;

        if (windowServerList != null)
        {
            EnsureTopmostCanvas(windowServerList, 1001);
            windowServerList.SetActive(true);

            BuildSessionListUI();
            OnRefreshClicked();
        }
    }

    private void BuildSessionListUI()
    {
        if (windowServerList == null) return;

        var canvas = windowServerList.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            EnsureTopmostCanvas(windowServerList, 1001);
            canvas = windowServerList.GetComponentInParent<Canvas>();
        }

        if (windowServerList.GetComponent<RectTransform>() == null)
            windowServerList.AddComponent<RectTransform>();

        // Background
        var bgImg = windowServerList.GetComponent<Image>();
        if (bgImg == null) bgImg = windowServerList.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // Title
        CreateTMP(windowServerList.transform, "Title", "Danh sách phòng",
            new Vector2(0, 250), new Vector2(500, 50), 30, TextAlignmentOptions.Center);

        // Back button
        var backObj = new GameObject("BackBtn", typeof(Image), typeof(Button), typeof(RectTransform));
        backObj.transform.SetParent(windowServerList.transform, false);
        var backRect = backObj.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 1);
        backRect.anchorMax = new Vector2(0, 1);
        backRect.pivot = new Vector2(0, 1);
        backRect.sizeDelta = new Vector2(100, 40);
        backRect.anchoredPosition = new Vector2(10, -10);
        backObj.GetComponent<Image>().color = new Color(0.4f, 0.2f, 0.2f, 1f);
        var backBtnLocal = backObj.GetComponent<Button>();
        backBtnLocal.onClick.AddListener(() => windowServerList.SetActive(false));
        CreateTMP(backObj.transform, "Label", "Quay lại", Vector2.zero, new Vector2(100, 40), 18, TextAlignmentOptions.Center, true);

        // Refresh button
        var refreshObj = new GameObject("RefreshBtn", typeof(Image), typeof(Button), typeof(RectTransform));
        refreshObj.transform.SetParent(windowServerList.transform, false);
        var refreshRect = refreshObj.GetComponent<RectTransform>();
        refreshRect.anchorMin = new Vector2(1, 1);
        refreshRect.anchorMax = new Vector2(1, 1);
        refreshRect.pivot = new Vector2(1, 1);
        refreshRect.sizeDelta = new Vector2(120, 40);
        refreshRect.anchoredPosition = new Vector2(-10, -10);
        refreshObj.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.2f, 1f);
        var refreshBtnLocal = refreshObj.GetComponent<Button>();
        refreshBtnLocal.onClick.AddListener(OnRefreshClicked);
        CreateTMP(refreshObj.transform, "Label", "Làm mới", Vector2.zero, new Vector2(120, 40), 18, TextAlignmentOptions.Center, true);

        // Scroll view for session list
        var scrollObj = new GameObject("SessionScroll", typeof(Image), typeof(RectTransform));
        scrollObj.transform.SetParent(windowServerList.transform, false);
        var scrollRect = scrollObj.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.05f, 0.08f);
        scrollRect.anchorMax = new Vector2(0.95f, 0.85f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
        scrollObj.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

        var scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        var contentObj = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        var contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        var fitter = contentObj.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;
        layout.spacing = 4;
        layout.padding = new RectOffset(8, 8, 8, 8);

        scroll.content = contentRect;

        // Scrollbar
        var scrollbarObj = new GameObject("Scrollbar", typeof(Image), typeof(Scrollbar), typeof(RectTransform));
        scrollbarObj.transform.SetParent(scrollObj.transform, false);
        var sbRect = scrollbarObj.GetComponent<RectTransform>();
        sbRect.anchorMin = new Vector2(1, 0);
        sbRect.anchorMax = new Vector2(1, 1);
        sbRect.pivot = new Vector2(1, 0.5f);
        sbRect.sizeDelta = new Vector2(16, 0);
        scrollbarObj.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);

        var scrollbar = scrollbarObj.GetComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        var sbHandle = new GameObject("Handle", typeof(Image), typeof(RectTransform));
        sbHandle.transform.SetParent(scrollbarObj.transform, false);
        var sbHandleRect = sbHandle.GetComponent<RectTransform>();
        sbHandleRect.anchorMin = Vector2.zero;
        sbHandleRect.anchorMax = new Vector2(1, 0.2f);
        sbHandleRect.sizeDelta = Vector2.zero;
        sbHandle.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f, 1f);
        scrollbar.handleRect = sbHandleRect;
        scrollbar.targetGraphic = sbHandle.GetComponent<Image>();

        scroll.verticalScrollbar = scrollbar;

        sessionListContent = contentObj.transform;

        // Empty label
        sessionListEmptyText = CreateTMP(windowServerList.transform, "EmptyLabel", "Đang tải...",
            new Vector2(0, 0), new Vector2(400, 40), 20, TextAlignmentOptions.Center);
    }

    private void PopulateSessionList()
    {
        if (sessionListContent == null) return;

        // Clear old entries
        for (int i = sessionListContent.childCount - 1; i >= 0; i--)
            Destroy(sessionListContent.GetChild(i).gameObject);

        int visibleCount = 0;
        foreach (var session in cachedSessions)
        {
            if (!session.IsVisible) continue;
            if (session.Name == "__lobby_browser__") continue;
            CreateSessionEntry(session);
            visibleCount++;
        }

        if (visibleCount == 0)
        {
            if (sessionListEmptyText != null)
            {
                sessionListEmptyText.gameObject.SetActive(true);
                sessionListEmptyText.text = "Không có phòng nào.\nNhấn \"Làm mới\" để tìm phòng.";
            }
            return;
        }

        if (sessionListEmptyText != null)
            sessionListEmptyText.gameObject.SetActive(false);
    }

    private void CreateSessionEntry(SessionInfo session)
    {
        var entryObj = new GameObject($"Session_{session.Name}", typeof(Image), typeof(Button), typeof(RectTransform), typeof(LayoutElement));
        entryObj.transform.SetParent(sessionListContent, false);

        var entryImg = entryObj.GetComponent<Image>();
        entryImg.color = new Color(0.22f, 0.22f, 0.22f, 1f);

        var layout = entryObj.GetComponent<LayoutElement>();
        layout.preferredHeight = 50;
        layout.minHeight = 50;

        var rect = entryObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.sizeDelta = new Vector2(0, 50);

        // Session name
        CreateTMP(entryObj.transform, "SessionName", session.Name,
            new Vector2(-180, 0), new Vector2(360, 40), 20, TextAlignmentOptions.MidlineLeft, true);

        // Player count
        CreateTMP(entryObj.transform, "PlayerCount", $"{session.PlayerCount}/{session.MaxPlayers}",
            new Vector2(180, 0), new Vector2(120, 40), 18, TextAlignmentOptions.MidlineRight, true);

        // Join button
        var joinBtnObj = new GameObject("JoinBtn", typeof(Image), typeof(Button), typeof(RectTransform));
        joinBtnObj.transform.SetParent(entryObj.transform, false);
        var joinRect = joinBtnObj.GetComponent<RectTransform>();
        joinRect.anchorMin = new Vector2(1, 0.5f);
        joinRect.anchorMax = new Vector2(1, 0.5f);
        joinRect.pivot = new Vector2(1, 0.5f);
        joinRect.sizeDelta = new Vector2(80, 34);
        joinRect.anchoredPosition = new Vector2(-8, 0);
        joinBtnObj.GetComponent<Image>().color = new Color(0.25f, 0.55f, 0.25f, 1f);

        CreateTMP(joinBtnObj.transform, "Label", "Tham gia", Vector2.zero, new Vector2(80, 34), 16, TextAlignmentOptions.Center, true);

        var joinButton = joinBtnObj.GetComponent<Button>();
        string sessionName = session.Name;
        joinButton.onClick.AddListener(() => JoinSession(sessionName));

        // Hover effect
        var btn = entryObj.GetComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        btn.colors = colors;
    }

    private void JoinSession(string sessionName)
    {
        Debug.Log($"[ServerUI] Joining session: {sessionName}");
        GameSessionData.SessionName = sessionName;
        GameSessionData.IsMultiplayer = true;
        GameSessionData.IsHost = false;

        if (windowServerList != null)
            windowServerList.SetActive(false);

        if (launcher != null)
            _ = launcher.LaunchAsClient(sessionName);
    }

    // ── Session List Callback ──

    private void OnSessionListUpdated(List<SessionInfo> sessions)
    {
        cachedSessions.Clear();
        cachedSessions.AddRange(sessions);
        PopulateSessionList();
    }

    // ── Disconnect / Error Handling ──

    private void OnDisconnected(string message)
    {
        Debug.LogWarning($"[ServerUI] Disconnected: {message}");
        ShowDisconnectOverlay(message);
    }

    private void OnConnectError(string message)
    {
        Debug.LogWarning($"[ServerUI] Connect error: {message}");
        ShowDisconnectOverlay(message);
    }

    private void CreateDisconnectOverlay()
    {
        var canvas = GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.transform : transform;

        disconnectOverlay = new GameObject("DisconnectOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        disconnectOverlay.transform.SetParent(parent, false);

        var rootRect = disconnectOverlay.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        var overlay = disconnectOverlay.GetComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.7f);
        overlay.raycastTarget = true;

        var panelObj = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObj.transform.SetParent(disconnectOverlay.transform, false);

        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(500, 200);
        panelRect.anchoredPosition = Vector2.zero;

        panelObj.GetComponent<Image>().color = new Color(0.18f, 0.14f, 0.12f, 1f);

        // Title
        CreateTMP(panelObj.transform, "Title", "Mất kết nối",
            new Vector2(0, 50), new Vector2(440, 40), 26, TextAlignmentOptions.Center);

        // Message
        var msgText = CreateTMP(panelObj.transform, "Message", "",
            new Vector2(0, 0), new Vector2(440, 60), 20, TextAlignmentOptions.Center);
        msgText.name = "DisconnectMessage";

        // OK button
        var okBtn = CreateButton(panelObj.transform, "OKBtn", "OK",
            new Vector2(0, -60), new Vector2(120, 44), new Color(0.3f, 0.5f, 0.3f, 1f));
        okBtn.onClick.AddListener(ReturnToMenu);

        disconnectOverlay.SetActive(false);
    }

    private void ShowDisconnectOverlay(string message)
    {
        if (disconnectOverlay == null) return;

        var msgText = disconnectOverlay.transform.Find("Panel/DisconnectMessage");
        if (msgText != null)
        {
            var tmp = msgText.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = message;
        }

        disconnectOverlay.SetActive(true);
        disconnectOverlay.transform.SetAsLastSibling();
    }

    private void ReturnToMenu()
    {
        if (disconnectOverlay != null)
            disconnectOverlay.SetActive(false);

        if (launcher != null)
            launcher.ShutdownRunner();

        GameSessionData.ResetSession();

        if (pendingAction == PendingAction.Join && windowJoin != null)
        {
            pendingAction = PendingAction.None;
            windowJoin.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("Scene_Menu");
        }
    }

    // ── Validation Message ──

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
        panelRect.sizeDelta = new Vector2(620, 180);
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

    private IEnumerator HideValidationMessageAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2.5f);

        HideValidationMessageNow();
        validationMessageRoutine = null;
    }

    // ── UI Helpers ──

    private TextMeshProUGUI CreateTMP(Transform parent, string objName, string text,
        Vector2 pos, Vector2 size, int fontSize, TextAlignmentOptions align, bool setWidth = false)
    {
        var go = new GameObject(objName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        if (setWidth) tmp.overflowMode = TextOverflowModes.Ellipsis;
        return tmp;
    }

    private Button CreateButton(Transform parent, string objName, string label,
        Vector2 pos, Vector2 size, Color bgColor)
    {
        var go = new GameObject(objName, typeof(Image), typeof(Button), typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
        go.GetComponent<Image>().color = bgColor;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();

        CreateTMP(go.transform, "Label", label, Vector2.zero, size, 20, TextAlignmentOptions.Center, true);
        return btn;
    }
}
