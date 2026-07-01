using UnityEngine;

public static class GameSessionData
{
    public static string SelectedMapScene = "floor_1";
    public static bool IsMultiplayer = false;
    public static bool IsHost = true;
    public static int SelectedCharacterIndex = 0;
    public static string SessionName = "AbyssFrontier";

    public static void ResetSession()
    {
        SelectedMapScene = "floor_1";
        IsMultiplayer = false;
        IsHost = true;
        SelectedCharacterIndex = 0;
        SessionName = "AbyssFrontier";
    }
}
