using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

/// <summary>
/// Quản lý màn hình thông báo chiến thắng khi người chơi tiêu diệt Boss.
/// Hỗ trợ cả Singleplayer và Multiplayer (Photon Fusion).
/// </summary>
public class BossVictoryUI : MonoBehaviour
{
    // Singleton để RPC từ NetworkPlayer có thể gọi trực tiếp
    public static BossVictoryUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private string spawnPointTag = "SpawnPoint";
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.35f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Hiển thị màn hình chiến thắng.
    /// Trong Singleplayer: gọi trực tiếp từ BossController.
    /// Trong Multiplayer: gọi qua RPC_ShowVictory() trên NetworkPlayer (broadcast tới All).
    /// </summary>
    public void ShowVictory()
    {
        Debug.Log("[BossVictoryUI] HIỂN THỊ MÀN HÌNH CHIẾN THẮNG!");

        if (winPanel != null)
            winPanel.SetActive(true);

        // Chỉ pause timeScale trong singleplayer.
        // Trong multiplayer, pause timeScale sẽ đóng băng Fusion simulation → lỗi mạng.
        if (!GameSessionData.IsMultiplayer)
            Time.timeScale = 0f;

        // Unlock tầng tiếp theo (mỗi máy tự unlock local)
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.StartsWith("floor"))
        {
            string numberPart = currentScene.Substring(5);
            if (int.TryParse(numberPart, out int floorNumber))
            {
                int nextFloorNumber = floorNumber + 1;
                string nextFloorScene = "floor" + nextFloorNumber;

                SaveManager.UnlockFloor(nextFloorScene);
                Debug.Log($"[BossVictoryUI] Automatically unlocked next floor: {nextFloorScene}");

                if (SaveManager.Instance != null)
                    _ = SaveManager.Instance.SaveGameAsync();
            }
        }
    }

    // ── Button Handlers ───────────────────────────────────────────────────────

    /// <summary>
    /// Nút "Again" — chơi lại màn hiện tại.
    /// Singleplayer: LaunchAsSingleplayer để reload scene + spawn lại.
    /// Multiplayer: client gửi RPC lên Host, Host relaunch session cho tất cả.
    /// </summary>
    public void OnAgainClicked()
    {
        if (winPanel != null) winPanel.SetActive(false);
        Time.timeScale = 1f;

        if (GameSessionData.IsMultiplayer)
        {
            // Yêu cầu Host restart — chỉ 1 client cần bấm, Host sẽ xử lý cho tất cả.
            // Tìm bất kỳ NetworkPlayer nào có InputAuthority để gửi RPC lên Host.
            NetworkPlayer localNetPlayer = FindLocalNetworkPlayer();
            if (localNetPlayer != null)
            {
                localNetPlayer.RPC_RequestRestart();
            }
            else
            {
                // Fallback nếu không tìm được NetworkPlayer (ví dụ đang là Host)
                string currentScene = SceneManager.GetActiveScene().name;
                var launcher = FindFirstObjectByType<GameLauncher>();
                if (launcher != null)
                {
                    launcher.ShutdownRunner();
                    if (GameSessionData.IsHost)
                        _ = launcher.LaunchAsHost(currentScene, GameSessionData.SessionName);
                    else
                        _ = launcher.LaunchAsClient(GameSessionData.SessionName);
                }
            }
        }
        else
        {
            // Singleplayer: reload scene thông qua GameLauncher để spawn lại đúng cách
            string currentScene = SceneManager.GetActiveScene().name;
            var launcher = FindFirstObjectByType<GameLauncher>();
            if (launcher != null)
                _ = launcher.LaunchAsSingleplayer(currentScene);
            else
                SceneManager.LoadScene(currentScene);
        }
    }

    /// <summary>
    /// Nút "Next" — về menu, chọn màn mới đã mở.
    /// </summary>
    public void OnNextClicked()
    {
        if (winPanel != null) winPanel.SetActive(false);
        Time.timeScale = 1f;

        // Shutdown Runner trước khi về menu (multiplayer)
        if (GameSessionData.IsMultiplayer)
        {
            var launcher = FindFirstObjectByType<GameLauncher>();
            launcher?.ShutdownRunner();
        }

        GameSessionData.OpenMapPanelNext = true;
        SceneManager.LoadScene("Scene_Menu");
    }

    /// <summary>
    /// Nút "Close" — về main menu.
    /// </summary>
    public void OnCloseClicked()
    {
        if (winPanel != null) winPanel.SetActive(false);
        Time.timeScale = 1f;

        // Shutdown Runner trước khi về menu (multiplayer)
        if (GameSessionData.IsMultiplayer)
        {
            var launcher = FindFirstObjectByType<GameLauncher>();
            launcher?.ShutdownRunner();
        }

        SceneManager.LoadScene("Scene_Menu");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static NetworkPlayer FindLocalNetworkPlayer()
    {
        NetworkPlayer[] all = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        foreach (var np in all)
        {
            if (np.Object != null && np.Object.HasInputAuthority)
                return np;
        }
        return null;
    }
}
