using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion;

public class LobbyUI : MonoBehaviour
{
    private static Sprite cachedWhiteSprite;

    private Canvas selfCanvas;
    private GameObject overlay;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI playerCountText;
    private TextMeshProUGUI charNameText;
    private Button actionBtn;
    private TextMeshProUGUI actionBtnLabel;
    private Button backBtn;

    private int selectedCharIndex;
    private NetworkLobby lobby;
    private bool isHost;
    private bool isReady;

    private void OnDestroy()
    {
        if (lobby != null)
            lobby.OnStateChanged -= OnLobbyStateChanged;
    }

    public void Show(bool host)
    {
        isHost = host;
        isReady = false;
        selectedCharIndex = GameSessionData.SelectedCharacterIndex;

        BuildUI();
        overlay.SetActive(true);

        lobby = FindLobby();
        if (lobby != null)
        {
            lobby.OnStateChanged += OnLobbyStateChanged;
            lobby.RPC_SetCharacter(lobby.Runner.LocalPlayer, selectedCharIndex);
            lobby.RPC_RequestState();
        }
        else
        {
            Debug.LogWarning("[LobbyUI] NetworkLobby not found, retrying...");
            StartCoroutine(RetryFindLobby());
        }

        Refresh();
    }

    public void Hide()
    {
        if (lobby != null)
        {
            lobby.OnStateChanged -= OnLobbyStateChanged;
            lobby = null;
        }
        if (overlay != null)
            overlay.SetActive(false);
    }

    public void ShowNotAllReadyPopup()
    {
        if (selfCanvas == null) return;

        var popup = new GameObject("NotReadyPopup", typeof(Image), typeof(Button));
        popup.transform.SetParent(selfCanvas.transform, false);

        var rect = popup.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400, 160);
        rect.anchoredPosition = Vector2.zero;

        var img = popup.GetComponent<Image>();
        img.sprite = WhiteSprite();
        img.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        var textObj = new GameObject("Message", typeof(RectTransform));
        textObj.transform.SetParent(popup.transform, false);
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Some players are not ready or have not selected a character yet!";
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        popup.GetComponent<Button>().onClick.AddListener(() => Destroy(popup));
        Destroy(popup, 3f);
    }

    private void BuildUI()
    {
        if (overlay != null)
            return;

        EnsureCanvas();

        overlay = new GameObject("LobbyOverlay", typeof(Image), typeof(RectTransform));
        overlay.transform.SetParent(selfCanvas.transform, false);

        var overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;

        var overlayImg = overlay.GetComponent<Image>();
        overlayImg.sprite = WhiteSprite();
        overlayImg.color = new Color(0f, 0f, 0f, 0.85f);
        overlayImg.raycastTarget = true;

        statusText = MakeTMP(overlay.transform, "StatusLabel", "Preparing lobby...",
            new Vector2(0, 210), new Vector2(500, 50), 26);

        charNameText = MakeTMP(overlay.transform, "CharName", "Character 1",
            new Vector2(-250, 50), new Vector2(300, 40), 24);

        playerCountText = MakeTMP(overlay.transform, "PlayerCount", "Players: 0 / 4",
            new Vector2(250, 50), new Vector2(300, 40), 22);

        actionBtn = MakeButton(overlay.transform, "ActionBtn", isHost ? "Start" : "Ready",
            new Vector2(0, -180), new Vector2(200, 50), OnActionClicked);
        actionBtnLabel = actionBtn.GetComponentInChildren<TextMeshProUGUI>();

        backBtn = MakeButton(overlay.transform, "BackBtn", "Back",
            new Vector2(0, -250), new Vector2(200, 50), OnBackClicked);

        MakeButton(overlay.transform, "PrevBtn", "<",
            new Vector2(-400, 50), new Vector2(60, 40), OnPrevCharacter);
        MakeButton(overlay.transform, "NextBtn", ">",
            new Vector2(-100, 50), new Vector2(60, 40), OnNextCharacter);

        Debug.Log("[LobbyUI] UI built successfully with TMP.");
    }

    private void EnsureCanvas()
    {
        selfCanvas = GetComponent<Canvas>();
        if (selfCanvas == null)
            selfCanvas = gameObject.AddComponent<Canvas>();

        selfCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        selfCanvas.overrideSorting = true;
        selfCanvas.sortingOrder = 9999;

        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    private void OnPrevCharacter()
    {
        selectedCharIndex--;
        Refresh();
        SyncChar();
    }

    private void OnNextCharacter()
    {
        selectedCharIndex++;
        Refresh();
        SyncChar();
    }

    private void OnActionClicked()
    {
        if (isHost)
        {
            if (selectedCharIndex < 0)
            {
                Debug.LogWarning("[LobbyUI] Pick a character before starting the game.");
                return;
            }

            if (lobby != null) lobby.TryStartGame();
        }
        else
        {
            isReady = !isReady;
            UpdateButtonLabel();
            if (lobby != null)
            {
                var player = lobby.Runner != null ? lobby.Runner.LocalPlayer : default;
                lobby.RPC_SetReady(player, isReady);
            }
        }
    }

    private void OnBackClicked()
    {
        if (lobby != null && lobby.Runner != null && lobby.Runner.IsRunning)
            lobby.Runner.Shutdown();

        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
            Destroy(launcher.gameObject);

        lobby = null;
        SceneManager.LoadScene("Scene_Menu");
    }

    private void Refresh()
    {
        if (statusText != null)
            statusText.text = lobby != null ? (isHost ? "Host Lobby" : "Player Lobby") : "Waiting for lobby...";

        if (charNameText != null)
            charNameText.text = selectedCharIndex >= 0 ? $"Character {selectedCharIndex + 1}" : "Select a character";

        if (lobby != null && playerCountText != null)
            playerCountText.text = $"Players: {lobby.PlayerCount} / 4";

        if (actionBtn != null)
            actionBtn.interactable = !isHost || selectedCharIndex >= 0;

        UpdateButtonLabel();
    }

    private void UpdateButtonLabel()
    {
        if (actionBtnLabel == null) return;
        actionBtnLabel.text = isHost
            ? (selectedCharIndex >= 0 ? "Start" : "Pick First")
            : (isReady ? "Ready!" : "Ready");
    }

    private void SyncChar()
    {
        if (charNameText != null)
            charNameText.text = $"Character {selectedCharIndex + 1}";

        if (lobby != null)
        {
            var player = lobby.Runner != null ? lobby.Runner.LocalPlayer : default;
            lobby.RPC_SetCharacter(player, selectedCharIndex);
        }
    }

    private void OnLobbyStateChanged()
    {
        Refresh();
    }

    private NetworkLobby FindLobby()
    {
        var found = NetworkLobby.Instance;
        if (found != null) return found;
        return FindFirstObjectByType<NetworkLobby>();
    }

    private IEnumerator RetryFindLobby()
    {
        float elapsed = 0f;
        while (lobby == null && elapsed < 10f)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;

            lobby = FindLobby();
            if (lobby != null)
            {
                Debug.Log("[LobbyUI] Found NetworkLobby after retry.");
                lobby.OnStateChanged += OnLobbyStateChanged;
                lobby.RPC_RequestState();
                Refresh();
                yield break;
            }
        }

        if (lobby == null)
        {
            Debug.LogError("[LobbyUI] NetworkLobby not found after retries.");
            if (statusText != null)
                statusText.text = "Lobby connection failed!";
        }
    }

    private TextMeshProUGUI MakeTMP(Transform parent, string objName, string text, Vector2 pos, Vector2 size, int fontSize)
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
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private Button MakeButton(Transform parent, string objName, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(objName, typeof(Image), typeof(Button), typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;

        var img = go.GetComponent<Image>();
        img.sprite = WhiteSprite();
        img.color = new Color(0.3f, 0.3f, 0.3f, 1);

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(go.transform, false);
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btn;
    }

    private static Sprite WhiteSprite()
    {
        if (cachedWhiteSprite != null)
            return cachedWhiteSprite;

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        texture.Apply();
        cachedWhiteSprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
        return cachedWhiteSprite;
    }
}
