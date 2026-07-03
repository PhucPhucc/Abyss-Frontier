using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform buttonContainer;

    private static readonly string[] AllFloors = {
        "floor1", "floor2", "floor3", "floor4", "floor5", "floor6"
    };

    private void OnEnable()
    {
        RefreshButtons();
    }

    public void RefreshButtons()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (string floor in AllFloors)
        {
            var btn = Instantiate(levelButtonPrefab, buttonContainer);
            var ui = btn.GetComponent<LevelSelectButton>();
            if (ui != null)
            {
                ui.Setup(floor, SaveManager.IsFloorUnlocked(floor));
            }
        }
    }

    public void LoadLevel(string sceneName)
    {
        if (!SaveManager.IsFloorUnlocked(sceneName)) return;
        string target = sceneName;
        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
            _ = launcher.LaunchAsSingleplayer(target);
        else
            SceneManager.LoadScene(target);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
