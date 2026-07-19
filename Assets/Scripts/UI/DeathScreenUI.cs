using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Màn hình thông báo khi Player chết. 
/// Cần được gán sẵn vào Canvas và kéo tham chiếu LosePanel vào inspector.
/// </summary>
[DisallowMultipleComponent]
public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject losePanel;
    [SerializeField] private string menuSceneName = "Scene_Menu";
    [SerializeField] private string spawnPointTag = "SpawnPoint";
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.35f);

    private PlayerHealth playerHealth;

    public static void ShowDeath(PlayerHealth player)
    {
        if (player == null) return;

        DeathScreenUI screen = FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (screen != null)
        {
            screen.Show(player);
        }
        else
        {
            Debug.LogError("Không tìm thấy DeathScreenUI trong Scene! Vui lòng đảm bảo bạn đã tạo GameObject và kéo script này vào.");
        }
    }

    public void Show(PlayerHealth player)
    {
        playerHealth = player;

        if (losePanel != null)
            losePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Gán hàm này vào sự kiện OnClick của nút "Again"
    /// Respawn player tại SpawnPoint hoặc Base_Camp thay vì load lại scene,
    /// tránh mất player do Fusion runner không re-spawn sau scene reload.
    /// </summary>
    public void OnAgainClicked()
    {
        Time.timeScale = 1f;

        if (losePanel != null)
            losePanel.SetActive(false);

        // Ưu tiên 1: hồi sinh tại Base_Camp nếu có trong scene
        Base_Camp baseCamp = FindFirstObjectByType<Base_Camp>();
        if (baseCamp != null)
        {
            if (playerHealth != null)
            {
                playerHealth.transform.position = (Vector2)baseCamp.transform.position + respawnOffset;
                playerHealth.Respawn();
            }
            return;
        }

        // Ưu tiên 2: hồi sinh tại SpawnPoint nếu có
        if (playerHealth != null)
        {
            GameObject spawnPoint = GameObject.FindGameObjectWithTag(spawnPointTag);
            if (spawnPoint != null)
                playerHealth.transform.position = (Vector2)spawnPoint.transform.position + respawnOffset;

            playerHealth.Respawn();
            return;
        }

        // Fallback: không tìm thấy player, đi qua GameLauncher để tránh mất spawn
        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
            _ = launcher.LaunchAsSingleplayer(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Gán hàm này vào sự kiện OnClick của nút "Close"
    /// </summary>
    public void OnCloseClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
