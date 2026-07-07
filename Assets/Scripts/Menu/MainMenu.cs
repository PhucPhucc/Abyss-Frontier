using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject ContinueButton;
    [SerializeField] private GameObject LevelSelectPanel;
    [SerializeField] private GameObject ConfirmPanel;

    private void Start()
    {
        if (CloudServiceManager.Instance != null)
            CloudServiceManager.Instance.AuthReady += OnAuthReady;
        else
            UpdateContinueButton();

        if (ContinueButton != null)
            ContinueButton.SetActive(false);
    }

    private void OnDestroy()
    {
        if (CloudServiceManager.Instance != null)
            CloudServiceManager.Instance.AuthReady -= OnAuthReady;
    }

    private void OnAuthReady()
    {
        UpdateContinueButton();
    }

    private void UpdateContinueButton()
    {
        if (ContinueButton != null)
            ContinueButton.SetActive(SaveManager.Instance != null && SaveManager.Instance.HasSavedGame);
    }

    public void PlayGame()
    {
        Debug.Log($"[MainMenu] PlayGame called. SaveManager.Instance={(SaveManager.Instance != null)}, CloudAuth={CloudServiceManager.Instance?.Auth?.UserId}");

        if (SaveManager.Instance != null && SaveManager.Instance.HasSavedGame)
        {
            SaveManager.Instance.ContinueGame();
            return;
        }

        if (GameSessionData.SelectedCharacterPrefab != null)
        {
            Debug.Log($"[MainMenu] Using MenuFlow selection: map={GameSessionData.SelectedMapScene}");
            var flow = FindFirstObjectByType<MenuFlowController>();
            if (flow != null)
            {
                flow.StartGame();
                return;
            }
        }

        SaveManager.Instance?.ContinueGame();
    }

    public void NewGame()
    {
        ClearSave();
        EnemyHealth.KilledEnemyIds.Clear();
        SaveManager.UnlockedFloors.Clear();
        SaveManager.UnlockedFloors.Add("floor1");
        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
            _ = launcher.LaunchAsSingleplayer("floor1");
        else
            SceneManager.LoadScene("floor1");
    }

    public void OpenLevelSelect()
    {
        if (LevelSelectPanel != null)
            LevelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        if (LevelSelectPanel != null)
            LevelSelectPanel.SetActive(false);
    }

    public void ExitGame()
    {
        ConfirmPanel.SetActive(true);
        
    }

    private void ClearSave()
    {
        if (SaveManager.Instance != null)
            SaveManager.ClearSavedDataFlag();
        string uid = CloudServiceManager.Instance?.Auth?.UserId;
        if (uid != null && PlayerPrefs.HasKey("DummySaveData_" + uid))
        {
            PlayerPrefs.DeleteKey("DummySaveData_" + uid);
            PlayerPrefs.Save();
        }
    }

    public void OpenSettings()
    {
        if (SettingsPanel != null)
            SettingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);
    }

    public void ConfirmExitByYes()
    {
        SaveManager.Instance?.SaveGame();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ConfirmExitByNo()
    {
        ConfirmPanel.SetActive(false);
    }
}