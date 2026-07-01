using System.Collections;
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
        Debug.Log("UIManager.MainMenu called - saving then loading Scene_Menu");
        Time.timeScale = 1f;
        StartCoroutine(SaveThenQuitRoutine());
    }

    private IEnumerator SaveThenQuitRoutine()
    {
        if (SaveManager.Instance != null)
        {
            var task = SaveManager.Instance.SaveGameAsync();
            yield return new WaitUntil(() => task.IsCompleted);
        }

        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
        {
            Debug.Log("[UIManager] Destroying stale GameLauncher before returning to menu");
            DestroyImmediate(launcher.gameObject);
        }

        SceneManager.LoadScene("Scene_Menu");
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