using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public SaveConfirmPopup saveConfirmPopup;

    private bool isPaused;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void MainMenu()
    {
        pausePanel.SetActive(false);

        if (saveConfirmPopup == null)
            saveConfirmPopup = new GameObject("SaveConfirmPopup").AddComponent<SaveConfirmPopup>();

        saveConfirmPopup.Show(OnMainMenuSaveYes, OnMainMenuSaveNo);
    }

    private void OnMainMenuSaveYes()
    {
        StartCoroutine(SaveThenGoToMenu());
    }

    private void OnMainMenuSaveNo()
    {
        GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        GameSessionData.ResetSession();

        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
            Destroy(launcher.gameObject);

        SceneManager.LoadScene("Scene_Menu");
    }

    private System.Collections.IEnumerator SaveThenGoToMenu()
    {
        if (SaveManager.Instance != null)
        {
            var task = SaveManager.Instance.SaveGameAsync();
            yield return new WaitUntil(() => task.IsCompleted);
            Debug.Log($"[UIManager] Save completed. HasSavedGame={SaveManager.Instance.HasSavedGame}");
        }
        else
        {
            Debug.LogWarning("[UIManager] SaveManager.Instance is null, skipping save");
        }

        GoToMainMenu();
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToPause()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void ShowWin()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ShowLose()
    {
        losePanel.SetActive(true);
        Time.timeScale = 0;
    }
}