using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuFlowController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject chooseMapPanel;
    [SerializeField] private GameObject playModePanel;
    [SerializeField] private GameObject hostJoinPanel;
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private CharacterSelectionUI characterSelectionUI;

    [Header("Navigation Buttons (Optional)")]
    [SerializeField] private Button mapNextButton;
    [SerializeField] private Button playModeNextButton;
    [SerializeField] private Button startMatchButton;

    private void Start()
    {
        CacheCharacterSelectionUI();
        ShowOnlyPanel(mainMenuPanel);

        if (mapNextButton != null) mapNextButton.interactable = false;
        if (playModeNextButton != null) playModeNextButton.interactable = false;
        if (startMatchButton != null) startMatchButton.interactable = false;

        if (hostJoinPanel != null)
            hostJoinPanel.SetActive(false);
    }

    private void CacheCharacterSelectionUI()
    {
        if (characterSelectionUI == null && characterSelectPanel != null)
        {
            characterSelectionUI = characterSelectPanel.GetComponentInChildren<CharacterSelectionUI>(true);
        }

        if (characterSelectionUI != null)
        {
            characterSelectionUI.Configure(this);
        }
    }

    private void EnsureCharacterSelectionPanel()
    {
        CacheCharacterSelectionUI();

        if (characterSelectPanel != null)
        {
            return;
        }

        characterSelectionUI = CharacterSelectionUI.CreateRuntimeCanvas(this);
        characterSelectPanel = characterSelectionUI.gameObject;
        characterSelectPanel.SetActive(false);
    }

    private void ShowOnlyPanel(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(mainMenuPanel == activePanel);
        if (chooseMapPanel != null) chooseMapPanel.SetActive(chooseMapPanel == activePanel);
        if (playModePanel != null) playModePanel.SetActive(playModePanel == activePanel);
        if (hostJoinPanel != null) hostJoinPanel.SetActive(hostJoinPanel == activePanel);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(characterSelectPanel == activePanel);
    }

    #region Step 1: Main Menu -> Choose Map
    public void OnPlayButtonClicked()
    {
        ShowOnlyPanel(chooseMapPanel);
    }

    public void BeginSingleplayerCharacterSelection(string sceneName)
    {
        GameSessionData.SelectedMapScene = sceneName;
        GameSessionData.IsMultiplayer = false;
        EnsureCharacterSelectionPanel();
        characterSelectionUI?.Configure(this);
        ShowOnlyPanel(characterSelectPanel);
    }
    #endregion

    #region Step 2: Choose Map -> Play Mode
    public void SelectMapIndex(int mapIndex)
    {
        GameSessionData.SelectedMapScene = $"floor_{mapIndex}";
        Debug.Log($"[MenuFlow] Map: {GameSessionData.SelectedMapScene}");

        if (mapNextButton != null) mapNextButton.interactable = true;
    }

    public void SelectMapSceneName(string sceneName)
    {
        GameSessionData.SelectedMapScene = sceneName;
        Debug.Log($"[MenuFlow] Map: {GameSessionData.SelectedMapScene}");

        if (mapNextButton != null) mapNextButton.interactable = true;
    }

    public void OnMapSelectionNextClicked()
    {
        ShowOnlyPanel(playModePanel);
    }

    public void BackToMainMenu()
    {
        ShowOnlyPanel(mainMenuPanel);
    }
    #endregion

    #region Step 3: Play Mode -> Host/Join or Character Select
    public void SelectPlayModeInt(int modeIndex)
    {
        if (modeIndex == 1)
        {
            GameSessionData.IsMultiplayer = true;
            ShowHostJoinPanel();
        }
        else
        {
            GameSessionData.IsMultiplayer = false;
            OnPlayModeNextClicked();
        }
    }

    public void SelectPlayMode(bool isMultiplayer)
    {
        if (isMultiplayer)
        {
            GameSessionData.IsMultiplayer = true;
            ShowHostJoinPanel();
        }
        else
        {
            GameSessionData.IsMultiplayer = false;
            OnPlayModeNextClicked();
        }
    }

    private void ShowHostJoinPanel()
    {
        if (hostJoinPanel != null)
        {
            var hostJoin = hostJoinPanel.GetComponent<HostJoinUI>();
            if (hostJoin != null) hostJoin.Show();
            ShowOnlyPanel(hostJoinPanel);
        }
        else
        {
            Debug.LogWarning("[MenuFlow] No HostJoinPanel assigned! Going to CharacterSelect directly.");
            OnPlayModeNextClicked();
        }
    }

    public void OnHostSelected()
    {
        OnPlayModeNextClicked();
    }

    public void OnJoinSelected()
    {
        OnPlayModeNextClicked();
    }

    public void OnHostJoinBack()
    {
        GameSessionData.IsMultiplayer = false;
        ShowOnlyPanel(playModePanel);
    }

    public void OnPlayModeNextClicked()
    {
        EnsureCharacterSelectionPanel();
        characterSelectionUI?.Configure(this);
        ShowOnlyPanel(characterSelectPanel);
    }

    public void BackToChooseMap()
    {
        ShowOnlyPanel(chooseMapPanel);
    }
    #endregion

    #region Step 4: Character Select -> Start Game
    public void SelectCharacterIndex(int charIndex)
    {
        GameSessionData.SelectedCharacterIndex = charIndex;
        Debug.Log($"[MenuFlow] Character index: {charIndex}");

        if (startMatchButton != null) startMatchButton.interactable = true;
    }

    public void BackToPlayMode()
    {
        ShowOnlyPanel(playModePanel);
    }

    public void StartGame()
    {
        string scene = GameSessionData.SelectedMapScene == "floor_1" ? "quiz" : GameSessionData.SelectedMapScene;
        Debug.Log($"[MenuFlow] Scene: {scene} | Multiplayer: {GameSessionData.IsMultiplayer} | CharIndex: {GameSessionData.SelectedCharacterIndex}");

        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher == null)
        {
            Debug.LogError("[MenuFlow] No GameLauncher found!");
            return;
        }

        if (GameSessionData.IsMultiplayer)
        {
            if (GameSessionData.IsHost)
                _ = launcher.LaunchAsHost(scene, GameSessionData.SessionName);
            else
                _ = launcher.LaunchAsClient(GameSessionData.SessionName);
        }
        else
        {
            _ = launcher.LaunchAsSingleplayer(scene);
        }
    }
    #endregion
}
