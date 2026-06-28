using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform buttonContainer;

    private static readonly string[] AllFloors = {
        "floor_1", "floor_2", "floor_3", "floor_4", "floor_5"
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
        if (sceneName == "floor_1")
            SceneManager.LoadScene("quiz");
        else
            SceneManager.LoadScene(sceneName);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
