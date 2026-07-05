using UnityEngine;
using UnityEngine.UI;

public class MapSelectUI : MonoBehaviour
{
    [SerializeField] private MenuFlowController menuFlowController;
    [SerializeField] private MapSelectButton[] mapButtons;

    private void Awake()
    {
        if (menuFlowController == null)
            menuFlowController = FindFirstObjectByType<MenuFlowController>();

        if (mapButtons == null || mapButtons.Length == 0)
            mapButtons = GetComponentsInChildren<MapSelectButton>(true);

        foreach (var mapButton in mapButtons)
        {
            if (mapButton == null)
                continue;

            var button = mapButton.GetComponent<Button>();
            if (button == null)
                continue;

            string scene = mapButton.SceneName;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectMap(scene));
        }
    }

    private void OnEnable()
    {
        RefreshSelection(GameSessionData.SelectedMapScene);
    }

    public void SelectMap(string sceneName)
    {
        RefreshSelection(sceneName);

        if (menuFlowController != null)
            menuFlowController.SelectMapSceneName(sceneName);
    }

    private void RefreshSelection(string sceneName)
    {
        if (mapButtons == null)
            return;

        foreach (var mapButton in mapButtons)
        {
            if (mapButton == null)
                continue;

            mapButton.SetSelected(mapButton.SceneName == sceneName);
        }
    }
}
