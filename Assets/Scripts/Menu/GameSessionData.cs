using UnityEngine;

public static class GameSessionData
{
    public static string SelectedMapScene = "floor_1";
    public static bool IsMultiplayer = false;
    public static bool IsHost = true;
    public static int SelectedCharacterIndex = 0;
    public static GameObject SelectedCharacterPrefab { get; private set; }
    public static string SessionName = "AbyssFrontier";

    public static void SelectCharacter(int characterIndex, CharacterData characterData)
    {
        SelectedCharacterIndex = characterIndex;
        SelectedCharacterPrefab = characterData != null ? characterData.PlayerPrefab : null;
    }

    public static void ResetSession()
    {
        SelectedMapScene = "floor_1";
        IsMultiplayer = false;
        IsHost = true;
        SelectedCharacterIndex = 0;
        SelectedCharacterPrefab = null;
        SessionName = "AbyssFrontier";
    }
}
