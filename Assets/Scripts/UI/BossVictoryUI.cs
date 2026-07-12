using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý màn hình thông báo chiến thắng khi người chơi tiêu diệt Boss tầng 5.
/// </summary>
public class BossVictoryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private string hubSceneName = "Scene_Menu";

    private void Awake()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    /// <summary>
    /// Kích hoạt hiển thị màn hình chiến thắng và tạm dừng thời gian game.
    /// </summary>
    public void ShowVictory()
    {
        Debug.Log("[BossVictoryUI] HIỂN THỊ MÀN HÌNH CHIẾN THẮNG!");
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        Time.timeScale = 0f; // Đóng băng trò chơi khi thắng

        // Tự động unlock tầng tiếp theo
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

                // Lưu game để lưu trạng thái unlocked floors mới
                if (SaveManager.Instance != null)
                {
                    _ = SaveManager.Instance.SaveGameAsync();
                }
            }
        }
    }

    /// <summary>
    /// Gọi bởi nút bấm "Return to Hub" trên UI.
    /// </summary>
    public void OnReturnToHubClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(hubSceneName);
    }

    /// <summary>
    /// Gọi bởi nút bấm "Quit to Main Menu".
    /// </summary>
    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scene_Menu");
    }
}
