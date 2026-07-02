using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private CharacterData[] characterDataArray;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private Button confirmButton;
    [SerializeField] private MenuFlowController menuFlowController;

    private CharacterCard[] cards;
    private int selectedIndex = -1;

    private void OnEnable()
    {
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        cards = new CharacterCard[characterDataArray.Length];
        for (int i = 0; i < characterDataArray.Length; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridContainer);
            CharacterCard card = cardObj.GetComponent<CharacterCard>();
            int index = i;
            card.Setup(i, characterDataArray[i], (idx) => SelectCharacter(idx));
            cards[i] = card;
        }

        selectedIndex = -1;
        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    public void SelectCharacter(int index)
    {
        if (selectedIndex >= 0 && selectedIndex < cards.Length)
            cards[selectedIndex].SetHighlight(false);

        selectedIndex = index;
        cards[selectedIndex].SetHighlight(true);

        if (menuFlowController != null)
            menuFlowController.SelectCharacterIndex(index);

        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    public void OnConfirmClicked()
    {
        if (selectedIndex >= 0 && menuFlowController != null)
            menuFlowController.StartGame();
    }
}
