using UnityEngine;
using UnityEngine.UI;

public class MapSelectButton : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private GameObject selectionBorder;

    public string SceneName => sceneName;

    private void Awake()
    {
        ConfigureBorderImage();
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.SetActive(selected);
    }

    private void ConfigureBorderImage()
    {
        if (selectionBorder == null)
            return;

        var image = selectionBorder.GetComponent<Image>();
        if (image == null)
            return;

        image.type = Image.Type.Sliced;
        image.fillCenter = false;
        image.raycastTarget = false;
    }
}
