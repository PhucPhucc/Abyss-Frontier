using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý màn hình thông báo chiến thắng khi người chơi tiêu diệt Boss tầng 5.
/// </summary>
public class BossVictoryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private string hubSceneName = "Scene_Menu";

    private void Awake()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    /// <summary>
    /// Kích hoạt hiển thị màn hình chiến thắng và tạm dừng thời gian game.
    /// </summary>
    public void ShowVictory()
    {
        Debug.Log("[BossVictoryUI] HIỂN THỊ MÀN HÌNH CHIẾN THẮNG!");
        if (winPanel != null)
        {
            winPanel.SetActive(true);
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
    /// Gọi bởi nút bấm "Again" trên UI. (Chơi lại màn hiện tại)
    /// </summary>
    public void OnAgainClicked()
    {
        if (winPanel != null) winPanel.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Gọi bởi nút bấm "Next" trên UI. (Chuyển sang map tiếp theo)
    /// </summary>
    public void OnNextClicked()
    {
        if (winPanel != null) winPanel.SetActive(false);
        Time.timeScale = 1f;
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene.StartsWith("floor"))
        {
            string numberPart = currentScene.Substring(5);
            if (int.TryParse(numberPart, out int floorNumber))
            {
                int nextFloorNumber = floorNumber + 1;
                string nextFloorScene = "floor" + nextFloorNumber;
                SceneManager.LoadScene(nextFloorScene);
                return;
            }
        }
        
        SceneManager.LoadScene("Scene_Menu");
    }

    /// <summary>
    /// Gọi bởi nút bấm "Close" trên UI. (Về Menu)
    /// </summary>
    public void OnCloseClicked()
    {
        if (winPanel != null) winPanel.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scene_Menu");
    }
}
