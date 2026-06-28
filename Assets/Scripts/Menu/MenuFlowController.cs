using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuFlowController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject chooseMapPanel;
    [SerializeField] private GameObject playModePanel;
    [SerializeField] private GameObject characterSelectPanel;

    [Header("Navigation Buttons (Optional)")]
    [SerializeField] private Button mapNextButton;
    [SerializeField] private Button playModeNextButton;
    [SerializeField] private Button startMatchButton;

    private void Start()
    {
        // Khởi tạo trạng thái ban đầu: chỉ hiện Main Menu, ẩn các Panel lựa chọn
        ShowOnlyPanel(mainMenuPanel);

        // Vô hiệu hóa nút Next cho đến khi người chơi chọn Map/Mode (nếu nút được gán)
        if (mapNextButton != null) mapNextButton.interactable = false;
        if (playModeNextButton != null) playModeNextButton.interactable = false;
        if (startMatchButton != null) startMatchButton.interactable = false;
    }

    private void ShowOnlyPanel(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(mainMenuPanel == activePanel);
        if (chooseMapPanel != null) chooseMapPanel.SetActive(chooseMapPanel == activePanel);
        if (playModePanel != null) playModePanel.SetActive(playModePanel == activePanel);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(characterSelectPanel == activePanel);
    }

    #region Step 1: Main Menu -> Choose Map
    /// <summary>
    /// Gọi khi ấn nút Play ở Main Menu
    /// </summary>
    public void OnPlayButtonClicked()
    {
        ShowOnlyPanel(chooseMapPanel);
    }
    #endregion

    #region Step 2: Choose Map -> Play Mode
    /// <summary>
    /// Gọi khi click vào 1 trong 5 image Map (gán tham số từ 1 đến 5 vào nút OnClick)
    /// </summary>
    public void SelectMapIndex(int mapIndex)
    {
        GameSessionData.SelectedMapScene = $"floor_{mapIndex}";
        Debug.Log($"[MenuFlow] Đã chọn Map: {GameSessionData.SelectedMapScene}");

        if (mapNextButton != null) mapNextButton.interactable = true;
    }

    /// <summary>
    /// Gọi khi click vào image Map (nếu muốn truyền trực tiếp tên scene, ví dụ "floor_1")
    /// </summary>
    public void SelectMapSceneName(string sceneName)
    {
        GameSessionData.SelectedMapScene = sceneName;
        Debug.Log($"[MenuFlow] Đã chọn Map: {GameSessionData.SelectedMapScene}");

        if (mapNextButton != null) mapNextButton.interactable = true;
    }

    /// <summary>
    /// Gọi khi ấn nút NextBTN ở màn hình chọn Map
    /// </summary>
    public void OnMapSelectionNextClicked()
    {
        ShowOnlyPanel(playModePanel);
    }

    /// <summary>
    /// Nút Quay lại từ màn chọn Map về Main Menu
    /// </summary>
    public void BackToMainMenu()
    {
        ShowOnlyPanel(mainMenuPanel);
    }
    #endregion

    #region Step 3: Play Mode -> Character Select
    /// <summary>
    /// Gọi khi chọn chế độ chơi: 0 = Singleplayer, 1 = Multiplayer
    /// </summary>
    public void SelectPlayModeInt(int modeIndex)
    {
        GameSessionData.IsMultiplayer = (modeIndex == 1);
        Debug.Log($"[MenuFlow] Đã chọn chế độ chơi: {(GameSessionData.IsMultiplayer ? "Multiplayer" : "Singleplayer")}");

        if (playModeNextButton != null) playModeNextButton.interactable = true;
    }

    /// <summary>
    /// Gọi khi chọn chế độ chơi (dùng bool boolean trong Unity Event)
    /// </summary>
    public void SelectPlayMode(bool isMultiplayer)
    {
        GameSessionData.IsMultiplayer = isMultiplayer;
        Debug.Log($"[MenuFlow] Đã chọn chế độ chơi: {(GameSessionData.IsMultiplayer ? "Multiplayer" : "Singleplayer")}");

        if (playModeNextButton != null) playModeNextButton.interactable = true;
    }

    /// <summary>
    /// Gọi khi ấn nút Next ở màn hình chọn Play Mode
    /// </summary>
    public void OnPlayModeNextClicked()
    {
        if (characterSelectPanel != null)
        {
            ShowOnlyPanel(characterSelectPanel);
        }
        else
        {
            // Nếu chưa có bảng chọn nhân vật thì có thể vào thẳng Game hoặc báo lỗi
            Debug.LogWarning("[MenuFlow] Chưa gán CharacterSelectPanel! Tiến hành vào thẳng Game...");
            StartGame();
        }
    }

    /// <summary>
    /// Nút Quay lại từ màn chọn Play Mode về màn chọn Map
    /// </summary>
    public void BackToChooseMap()
    {
        ShowOnlyPanel(chooseMapPanel);
    }
    #endregion

    #region Step 4: Character Select -> Start Game
    /// <summary>
    /// Gọi khi click vào thẻ nhân vật (0: Player, 1: Player_2, 2: Player_3)
    /// </summary>
    public void SelectCharacterIndex(int charIndex)
    {
        GameSessionData.SelectedCharacterIndex = charIndex;
        Debug.Log($"[MenuFlow] Đã chọn nhân vật index: {charIndex}");

        if (startMatchButton != null) startMatchButton.interactable = true;
    }

    /// <summary>
    /// Nút Quay lại từ màn chọn nhân vật về màn chọn Play Mode
    /// </summary>
    public void BackToPlayMode()
    {
        ShowOnlyPanel(playModePanel);
    }

    /// <summary>
    /// Gọi khi ấn nút BẮT ĐẦU (Start) để vào game
    /// </summary>
    public void StartGame()
    {
        Debug.Log($"[MenuFlow] Loading Scene: {GameSessionData.SelectedMapScene} | Mode: {(GameSessionData.IsMultiplayer ? "Multi" : "Single")} | CharIndex: {GameSessionData.SelectedCharacterIndex}");
        SceneManager.LoadScene(GameSessionData.SelectedMapScene);
    }
    #endregion
}
