using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILeaderboardService
{
    Task<bool> SubmitScore(string userId, string displayName, int score);
    Task<List<LeaderboardEntry>> GetTopScores(int count = 10);
    Task<int> GetPlayerRank(string userId);
}

public struct LeaderboardEntry
{
    public string UserId;
    public string DisplayName;
    public int Score;
    public int Rank;
}
