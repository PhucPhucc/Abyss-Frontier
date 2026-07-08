using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private SaveConfirmPopup saveConfirmPopup;

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
        Time.timeScale = 1f;
        GameSessionData.ResetSession();

        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
        {
            Debug.Log("[PauseManager] Destroying stale GameLauncher before returning to menu");
            Destroy(launcher.gameObject);
        }

        SceneManager.LoadScene("Scene_Menu");
    }

    private IEnumerator SaveThenGoToMenu()
    {
        if (SaveManager.Instance != null)
        {
            var task = SaveManager.Instance.SaveGameAsync();
            yield return new WaitUntil(() => task.IsCompleted);
        }

        GoToMainMenu();
    }
}