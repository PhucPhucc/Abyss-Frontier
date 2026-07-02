using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterCard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private Text nameText;
    [SerializeField] private GameObject highlightObject;

    private int characterIndex;
    private System.Action<int> onSelected;

    public void Setup(int index, CharacterData data, System.Action<int> callback)
    {
        characterIndex = index;
        portraitImage.sprite = data.PortraitSprite;
        nameText.text = data.CharacterName;
        onSelected = callback;
        SetHighlight(false);
    }

    public void SetHighlight(bool active)
    {
        if (highlightObject != null)
            highlightObject.SetActive(active);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSelected?.Invoke(characterIndex);
    }
}
