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
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.35f);

    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (losePanel != null)
            losePanel.SetActive(false);
    }

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
    /// </summary>
    public void OnAgainClicked()
    {
        Time.timeScale = 1f;

        if (losePanel != null)
            losePanel.SetActive(false);

        Base_Camp baseCamp = FindFirstObjectByType<Base_Camp>();
        if (baseCamp == null)
        {
            // Không có Base Camp, load lại scene hiện tại
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // Có Base Camp, hồi sinh tại đó
        if (playerHealth != null)
        {
            playerHealth.transform.position = (Vector2)baseCamp.transform.position + respawnOffset;
            playerHealth.Respawn();
        }
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
