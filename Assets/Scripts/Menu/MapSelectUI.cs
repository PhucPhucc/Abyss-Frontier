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
            button.onClick.AddListener(() => {
                if (SaveManager.IsFloorUnlocked(scene))
                {
                    SelectMap(scene);
                }
            });
        }
    }

    private void OnEnable()
    {
        RefreshSelection(GameSessionData.SelectedMapScene);
        UpdateButtonsInteractable();
    }

    private void UpdateButtonsInteractable()
    {
        if (mapButtons == null)
            return;

        foreach (var mapButton in mapButtons)
        {
            if (mapButton == null)
                continue;

            var button = mapButton.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = SaveManager.IsFloorUnlocked(mapButton.SceneName);
            }
        }
    }

    public void SelectMap(string sceneName)
    {
        if (!SaveManager.IsFloorUnlocked(sceneName))
            return;

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
