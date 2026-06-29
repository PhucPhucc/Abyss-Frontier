using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool isPaused;

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
        Debug.Log("PauseManager.MainMenu called - saving then quitting");
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

        var launcher = FindObjectOfType<GameLauncher>();
        if (launcher != null)
        {
            Debug.Log("[PauseManager] Destroying stale GameLauncher before returning to menu");
            Destroy(launcher.gameObject);
        }

        SceneManager.LoadScene("Scene_Menu");
    }
}