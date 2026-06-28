using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text label;
    [SerializeField] private GameObject lockIcon;

    private string sceneName;

    public void Setup(string floorName, bool unlocked)
    {
        sceneName = floorName;
        label.text = FormatFloorName(floorName);
        button.interactable = unlocked;
        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);
    }

    public void OnClick()
    {
        var parent = GetComponentInParent<LevelSelectUI>();
        if (parent != null)
            parent.LoadLevel(sceneName);
    }

    private static string FormatFloorName(string floor)
    {
        return floor.Replace("_", " ").ToUpper();
    }
}
