using UnityEngine;

public static class GameSessionData
{
    // Tên scene của Map được chọn (mặc định là floor_1)
    public static string SelectedMapScene = "floor_1";

    // Chế độ chơi: false = Singleplayer, true = Multiplayer
    public static bool IsMultiplayer = false;

    // Chỉ số nhân vật được chọn (0: Player, 1: Player_2, 2: Player_3...)
    public static int SelectedCharacterIndex = 0;

    // Đặt lại giá trị mặc định nếu cần
    public static void ResetSession()
    {
        SelectedMapScene = "floor_1";
        IsMultiplayer = false;
        SelectedCharacterIndex = 0;
    }
}
