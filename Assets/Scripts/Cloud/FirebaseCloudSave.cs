#if FB_SDK
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
#endif
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseCloudSave : IAuthService, ICloudSaveService, ILeaderboardService
{
#if FB_SDK
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private TaskCompletionSource<bool> initTcs = new TaskCompletionSource<bool>();
#endif
    private string _userId;

    public bool IsLoggedIn
    {
        get
        {
#if FB_SDK
            return auth != null && auth.CurrentUser != null;
#else
            return false;
#endif
        }
    }

    public string UserId => _userId;
    public string DisplayName
    {
        get
        {
#if FB_SDK
            return auth?.CurrentUser?.DisplayName ?? "";
#else
            return "";
#endif
        }
    }

    public bool IsReady { get; private set; }

    public Task WaitForInit()
    {
#if FB_SDK
        return initTcs.Task;
#else
        return Task.CompletedTask;
#endif
    }

    public void Initialize()
    {
#if FB_SDK
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase ready");
                IsReady = true;
                initTcs.TrySetResult(true);
            }
            else
            {
                Debug.LogError($"Firebase init failed: {task.Result}");
                initTcs.TrySetResult(false);
            }
        });
#endif
    }

    public async Task<AuthResult> LoginAnonymously()
    {
#if FB_SDK
        var result = await auth.SignInAnonymouslyAsync();
        _userId = result.User.UserId;
        return new AuthResult { Success = true, UserId = _userId };
#else
        await Task.Yield();
        return new AuthResult { Success = false, ErrorMessage = "FB_SDK not defined" };
#endif
    }

    public async Task<AuthResult> LoginWithEmail(string email, string password)
    {
#if FB_SDK
        var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
        _userId = result.User.UserId;
        return new AuthResult { Success = true, UserId = _userId };
#else
        await Task.Yield();
        return new AuthResult { Success = false, ErrorMessage = "FB_SDK not defined" };
#endif
    }

    public async Task<AuthResult> RegisterWithEmail(string email, string password)
    {
#if FB_SDK
        var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
        _userId = result.User.UserId;
        return new AuthResult { Success = true, UserId = _userId };
#else
        await Task.Yield();
        return new AuthResult { Success = false, ErrorMessage = "FB_SDK not defined" };
#endif
    }

    public void Logout()
    {
#if FB_SDK
        auth.SignOut();
#endif
        _userId = null;
    }

    public async Task<bool> SavePlayerData(string userId, string json)
    {
#if FB_SDK
        var doc = db.Collection("users").Document(userId);
        var data = new Dictionary<string, object> { { "data", json }, { "updatedAt", Timestamp.GetCurrentTimestamp() } };
        await doc.SetAsync(data);
        return true;
#else
        await Task.Yield();
        return false;
#endif
    }

    public async Task<string> LoadPlayerData(string userId)
    {
#if FB_SDK
        var doc = await db.Collection("users").Document(userId).GetSnapshotAsync();
        return doc.Exists && doc.TryGetValue("data", out string json) ? json : null;
#else
        await Task.Yield();
        return null;
#endif
    }

    public async Task<bool> SavePlayerStats(string userId, int level, int exp, int[] stats)
    {
#if FB_SDK
        var doc = db.Collection("users").Document(userId);
        var data = new Dictionary<string, object> { { "level", level }, { "exp", exp }, { "stats", stats } };
        await doc.UpdateAsync(data);
        return true;
#else
        await Task.Yield();
        return false;
#endif
    }

    public async Task<bool> SubmitScore(string userId, string displayName, int score)
    {
#if FB_SDK
        var doc = db.Collection("leaderboard").Document(userId);
        await doc.SetAsync(new Dictionary<string, object> { { "name", displayName }, { "score", score } });
        return true;
#else
        await Task.Yield();
        return false;
#endif
    }

    public async Task<List<LeaderboardEntry>> GetTopScores(int count = 10)
    {
#if FB_SDK
        var query = db.Collection("leaderboard").OrderByDescending("score").Limit(count);
        var snap = await query.GetSnapshotAsync();
        var list = new List<LeaderboardEntry>();
        int rank = 1;
        foreach (var doc in snap.Documents)
        {
            list.Add(new LeaderboardEntry
            {
                UserId = doc.Id,
                DisplayName = doc.GetValue<string>("name"),
                Score = doc.GetValue<int>("score"),
                Rank = rank++
            });
        }
        return list;
#else
        await Task.Yield();
        return new List<LeaderboardEntry>();
#endif
    }

    public async Task<int> GetPlayerRank(string userId)
    {
#if FB_SDK
        var all = await db.Collection("leaderboard").OrderByDescending("score").GetSnapshotAsync();
        int rank = 1;
        foreach (var doc in all.Documents)
        {
            if (doc.Id == userId) return rank;
            rank++;
        }
        return -1;
#else
        await Task.Yield();
        return -1;
#endif
    }
}
