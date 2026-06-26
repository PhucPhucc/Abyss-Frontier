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
    Time.timeScale = 1f;
    SceneManager.LoadScene("Scene_Menu");
    }
}