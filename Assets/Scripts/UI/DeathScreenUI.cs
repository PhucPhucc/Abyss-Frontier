using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

/// <summary>
/// Màn hình thông báo khi Player chết.
/// 
/// Singleplayer:
///   - Pause timeScale, chỉ người đang chơi thấy.
///   - Again: respawn tại chỗ hoặc reload scene.
///
/// Multiplayer (1 người chết → TẤT CẢ cùng thua):
///   - KHÔNG pause timeScale (Fusion cần chạy liên tục).
///   - PlayerHealth.Died → RPC_NotifyPlayerDied → Host → RPC_ShowLose (All).
///   - Again: restart toàn bộ session (giống Win/Again).
///   - Close: ShutdownRunner rồi về menu.
/// </summary>
[DisallowMultipleComponent]
public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject losePanel;
    [SerializeField] private string menuSceneName = "Scene_Menu";
    [SerializeField] private string spawnPointTag = "SpawnPoint";
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.35f);

    private PlayerHealth playerHealth;
    private bool isPaused = false;

    // ── Static entry points ───────────────────────────────────────────────────

    /// <summary>
    /// Dùng cho Singleplayer — gọi từ PlayerHealth.DeathSequenceRoutine() cục bộ.
    /// Trong Multiplayer, flow chạy qua RPC_ShowLose() → ShowMultiplayerLose().
    /// </summary>
    public static void ShowDeath(PlayerHealth player)
    {
        if (player == null) return;

        // Trong multiplayer, không dùng path này nữa — mọi thứ đi qua RPC.
        // Giữ lại để singleplayer và fallback hoạt động.
        if (GameSessionData.IsMultiplayer)
            return;

        DeathScreenUI screen = FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (screen != null)
            screen.ShowSingleplayer(player);
        else
            Debug.LogError("[DeathScreenUI] Không tìm thấy DeathScreenUI trong Scene!");
    }

    // ── Instance show methods ─────────────────────────────────────────────────

    /// <summary>
    /// Singleplayer: hiển thị màn hình thua và pause game.
    /// </summary>
    public void ShowSingleplayer(PlayerHealth player)
    {
        playerHealth = player;

        if (losePanel != null)
            losePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    /// <summary>
    /// Multiplayer: được gọi từ RPC_ShowLose() trên tất cả client.
    /// KHÔNG pause timeScale. Gán playerHealth cục bộ để UI có thể dùng nếu cần.
    /// </summary>
    public void ShowMultiplayerLose(PlayerHealth localPlayer)
    {
        playerHealth = localPlayer;
        isPaused = false;

        if (losePanel != null)
            losePanel.SetActive(true);
    }

    // ── Button: Again ─────────────────────────────────────────────────────────

    public void OnAgainClicked()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }

        if (losePanel != null)
            losePanel.SetActive(false);

        if (GameSessionData.IsMultiplayer)
        {
            // ── Multiplayer: restart toàn session (giống Win) ─────────────────
            // Gửi RPC lên Host → Host relaunch → tất cả client kết nối lại.
            NetworkPlayer localNetPlayer = FindLocalNetworkPlayer();
            if (localNetPlayer != null)
            {
                localNetPlayer.RPC_RequestRestart();
            }
            else
            {
                // Fallback nếu không tìm được NetworkPlayer (Host tự restart)
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
            // ── Singleplayer: respawn hoặc reload scene ───────────────────────

            // Ưu tiên 1: hồi sinh tại Base_Camp nếu có trong scene
            Base_Camp baseCamp = FindFirstObjectByType<Base_Camp>();
            if (baseCamp != null)
            {
                RespawnPlayer((Vector2)baseCamp.transform.position + respawnOffset);
                return;
            }

            // Ưu tiên 2: hồi sinh tại SpawnPoint nếu có
            if (playerHealth != null)
            {
                GameObject spawnPoint = GameObject.FindGameObjectWithTag(spawnPointTag);
                Vector2 respawnPosition = spawnPoint != null
                    ? (Vector2)spawnPoint.transform.position + respawnOffset
                    : (Vector2)playerHealth.transform.position;
                RespawnPlayer(respawnPosition);
                return;
            }

            // Fallback: không tìm thấy player, reload scene
            var launcher = FindFirstObjectByType<GameLauncher>();
            if (launcher != null)
                _ = launcher.LaunchAsSingleplayer(SceneManager.GetActiveScene().name);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void RespawnPlayer(Vector2 respawnPosition)
    {
        if (playerHealth == null)
            return;

        NetworkPlayer networkPlayer = playerHealth.GetComponent<NetworkPlayer>()
            ?? playerHealth.GetComponentInParent<NetworkPlayer>();

        if (GameSessionData.IsMultiplayer && networkPlayer != null)
        {
            networkPlayer.RPC_RequestRespawn(respawnPosition);
            return;
        }

        playerHealth.transform.position = respawnPosition;
        playerHealth.Respawn();
    }

    // ── Button: Close (về menu) ───────────────────────────────────────────────

    public void OnCloseClicked()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }

        if (GameSessionData.IsMultiplayer)
        {
            var launcher = FindFirstObjectByType<GameLauncher>();
            launcher?.ShutdownRunner();
        }

        SceneManager.LoadScene(menuSceneName);
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
