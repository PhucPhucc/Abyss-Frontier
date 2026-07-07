using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private CharacterData[] characterDataArray;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text counterText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private MenuFlowController menuFlowController;


    private int selectedIndex = -1;
    private MenuFlowController FlowController => menuFlowController != null ? menuFlowController : FindFirstObjectByType<MenuFlowController>();

    public int SelectedIndex => selectedIndex;
    public CharacterData SelectedCharacter => HasValidSelection ? characterDataArray[selectedIndex] : null;

    private void OnEnable()
    {
        RegisterButtonHandlers();
        SelectCharacter(HasCharacters ? Mathf.Clamp(selectedIndex, 0, characterDataArray.Length - 1) : -1);
    }

    private void OnDisable()
    {
        UnregisterButtonHandlers();
    }

    public void ShowPreviousCharacter()
    {
        if (!HasCharacters)
        {
            SelectCharacter(-1);
            return;
        }

        SelectCharacter(WrapIndex(selectedIndex - 1));
    }

    public void ShowNextCharacter()
    {
        if (!HasCharacters)
        {
            SelectCharacter(-1);
            return;
        }

        SelectCharacter(WrapIndex(selectedIndex + 1));
    }

    public void SelectCharacter(int index)
    {
        if (!HasCharacters)
        {
            selectedIndex = -1;
            UpdatePreview(null);
            UpdateButtonStates();
            return;
        }

        selectedIndex = WrapIndex(index);
        CharacterData characterData = characterDataArray[selectedIndex];

        UpdatePreview(characterData);
        UpdateButtonStates();

        if (menuFlowController != null)
            menuFlowController.SelectCharacter(selectedIndex, characterData);
    }

    public void OnConfirmClicked()
    {
        if (!HasValidSelection)
            return;

        menuFlowController?.StartGame();
    }

    private bool HasCharacters => characterDataArray != null && characterDataArray.Length > 0;
    private bool HasValidSelection => HasCharacters && selectedIndex >= 0 && selectedIndex < characterDataArray.Length;

    private int WrapIndex(int index)
    {
        if (!HasCharacters)
            return -1;

        int count = characterDataArray.Length;
        return (index % count + count) % count;
    }

    private void UpdatePreview(CharacterData characterData)
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = characterData != null ? characterData.PortraitSprite : null;
            portraitImage.enabled = characterData != null && characterData.PortraitSprite != null;
            portraitImage.preserveAspect = true;
        }

        if (nameText != null)
            nameText.text = GetDisplayName(characterData);

        if (counterText != null)
            counterText.text = HasValidSelection ? $"{selectedIndex + 1} / {characterDataArray.Length}" : "0 / 0";
    }

    private void UpdateButtonStates()
    {
        bool canPick = HasCharacters;
        bool canCycle = characterDataArray != null && characterDataArray.Length > 1;

        if (previousButton != null)
            previousButton.interactable = canCycle;

        if (nextButton != null)
            nextButton.interactable = canCycle;

        if (confirmButton != null)
            confirmButton.interactable = canPick;
    }

    private string GetDisplayName(CharacterData characterData)
    {
        if (characterData == null)
            return string.Empty;

        return string.IsNullOrWhiteSpace(characterData.CharacterName)
            ? characterData.name
            : characterData.CharacterName;
    }

    private void RegisterButtonHandlers()
    {
        if (previousButton != null)
        {
            previousButton.onClick.RemoveListener(ShowPreviousCharacter);
            previousButton.onClick.AddListener(ShowPreviousCharacter);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(ShowNextCharacter);
            nextButton.onClick.AddListener(ShowNextCharacter);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
    }

    private void UnregisterButtonHandlers()
    {
        if (previousButton != null)
            previousButton.onClick.RemoveListener(ShowPreviousCharacter);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(ShowNextCharacter);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
    }
}
