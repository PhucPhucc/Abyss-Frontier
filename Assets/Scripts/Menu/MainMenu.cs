using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject SettingsPanel;
    public GameObject ContinueButton;
    public GameObject LevelSelectPanel;

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
        SaveManager.Instance?.ContinueGame();
    }

    public void NewGame()
    {
        ClearSave();
        EnemyHealth.KilledEnemyIds.Clear();
        SaveManager.UnlockedFloors.Clear();
        SaveManager.UnlockedFloors.Add("floor_1");
        SceneManager.LoadScene("floor_1");
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
        SaveManager.Instance?.SaveGame();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ClearSave()
    {
        string uid = CloudServiceManager.Instance?.Auth?.UserId;
        if (uid != null && PlayerPrefs.HasKey("DummySaveData_" + uid))
        {
            PlayerPrefs.DeleteKey("DummySaveData_" + uid);
            PlayerPrefs.Save();
        }
    }

    public void OpenSettings()
    {
        SettingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
    }
}