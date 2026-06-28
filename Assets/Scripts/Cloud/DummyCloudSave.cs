using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DummyCloudSave : ICloudSaveService, IAuthService, ILeaderboardService
{
    private const string UID_KEY = "DummyUserId";
    private const string DATA_KEY = "DummySaveData_";

    public bool IsLoggedIn { get; private set; }
    public string UserId { get; private set; }
    public string DisplayName { get; private set; }

    public async Task<AuthResult> LoginAnonymously()
    {
        await Task.Yield();
        if (!PlayerPrefs.HasKey(UID_KEY))
            PlayerPrefs.SetString(UID_KEY, System.Guid.NewGuid().ToString());

        UserId = PlayerPrefs.GetString(UID_KEY);
        DisplayName = $"Player_{UserId.Substring(0, 6)}";
        IsLoggedIn = true;
        return new AuthResult { Success = true, UserId = UserId, DisplayName = DisplayName };
    }

    public async Task<AuthResult> LoginWithEmail(string email, string password)
    {
        await Task.Yield();
        return await LoginAnonymously();
    }

    public async Task<AuthResult> RegisterWithEmail(string email, string password)
    {
        await Task.Yield();
        return await LoginAnonymously();
    }

    public void Logout()
    {
        IsLoggedIn = false;
        UserId = null;
    }

    public async Task<bool> SavePlayerData(string userId, string json)
    {
        await Task.Yield();
        PlayerPrefs.SetString(DATA_KEY + userId, json);
        PlayerPrefs.Save();
        return true;
    }

    public async Task<string> LoadPlayerData(string userId)
    {
        await Task.Yield();
        return PlayerPrefs.GetString(DATA_KEY + userId, null);
    }

    public async Task<bool> SavePlayerStats(string userId, int level, int exp, int[] stats)
    {
        await Task.Yield();
        string json = JsonUtility.ToJson(new StatsData { level = level, exp = exp, stats = stats });
        return await SavePlayerData(userId, json);
    }

    public async Task<bool> SubmitScore(string userId, string displayName, int score)
    {
        await Task.Yield();
        PlayerPrefs.SetInt($"DummyScore_{userId}", score);
        PlayerPrefs.Save();
        return true;
    }

    public async Task<List<LeaderboardEntry>> GetTopScores(int count = 10)
    {
        await Task.Yield();
        return new List<LeaderboardEntry>
        {
            new LeaderboardEntry { UserId = UserId, DisplayName = DisplayName, Score = PlayerPrefs.GetInt($"DummyScore_{UserId}", 0), Rank = 1 }
        };
    }

    public async Task<int> GetPlayerRank(string userId)
    {
        await Task.Yield();
        return 1;
    }

    private class StatsData { public int level; public int exp; public int[] stats; }
}
