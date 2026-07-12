using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text label;
    [SerializeField] private GameObject lockIcon;

    private string sceneName;
    private bool unlocked;

    public void Setup(string floorName, bool isUnlocked)
    {
        sceneName = floorName;
        unlocked = isUnlocked;
        label.text = FormatFloorName(floorName);
        button.interactable = isUnlocked;
        if (lockIcon != null)
            lockIcon.SetActive(!isUnlocked);

        if (isUnlocked)
        {
            bool hasSave = SaveManager.Instance != null && SaveManager.HasSaveForMap(sceneName);
            label.text = FormatFloorName(floorName) + (hasSave ? " [S]" : "");
        }
    }

    public void OnClick()
    {
        if (!unlocked) return;

        GameSessionData.SelectedMapScene = sceneName;
        var ui = GetComponentInParent<LevelSelectUI>();
        if (ui != null)
            ui.Close();
    }

    private static string FormatFloorName(string floor)
    {
        return floor.Replace("_", " ").ToUpper();
    }
}
