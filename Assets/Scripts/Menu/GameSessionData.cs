using UnityEngine;

public static class GameSessionData
{
    private const int MinSessionNameLength = 3;
    private const int MaxSessionNameLength = 32;

    public static string SelectedMapScene = "floor1";
    public static bool IsMultiplayer = false;
    public static bool IsHost = true;
    public static int SelectedCharacterIndex = 0;
    public static GameObject SelectedCharacterPrefab { get; private set; }
    public static string SessionName = "AbyssFrontier";
    public static bool OpenMapPanelNext = false;

    public static void SelectCharacter(int characterIndex, CharacterData characterData)
    {
        SelectedCharacterIndex = characterIndex;
        SelectedCharacterPrefab = characterData != null ? characterData.PlayerPrefab : null;
        Debug.Log($"[GameSessionData] SelectCharacter index={characterIndex}, prefab={(SelectedCharacterPrefab != null ? SelectedCharacterPrefab.name : "NULL")}, data={(characterData != null ? characterData.name : "null")}");
    }

    public static void ResetSession()
    {
        SelectedMapScene = "floor1";
        IsMultiplayer = false;
        IsHost = true;
        SelectedCharacterIndex = 0;
        SelectedCharacterPrefab = null;
        SessionName = "AbyssFrontier";
    }

    public static bool TryValidateSessionName(string sessionName, out string normalizedName, out string errorMessage)
    {
        normalizedName = sessionName != null ? sessionName.Trim() : string.Empty;

        if (string.IsNullOrEmpty(normalizedName))
        {
            errorMessage = "Session name is empty.";
            return false;
        }

        if (normalizedName.Length < MinSessionNameLength)
        {
            errorMessage = $"Session name must be at least {MinSessionNameLength} characters.";
            return false;
        }

        if (normalizedName.Length > MaxSessionNameLength)
        {
            errorMessage = $"Session name must be at most {MaxSessionNameLength} characters.";
            return false;
        }

        for (int i = 0; i < normalizedName.Length; i++)
        {
            if (char.IsControl(normalizedName[i]))
            {
                errorMessage = "Session name cannot contain control characters.";
                return false;
            }
        }

        errorMessage = null;
        return true;
    }
}
