using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/CharacterData")]
public class CharacterData : ScriptableObject
{
    [SerializeField] private string characterName;
    [SerializeField] private Sprite portraitSprite;
    [SerializeField] private GameObject playerPrefab;

    public string CharacterName => characterName;
    public Sprite PortraitSprite => portraitSprite;
    public GameObject PlayerPrefab => playerPrefab;
}
