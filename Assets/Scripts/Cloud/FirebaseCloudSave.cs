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
            if (task.IsFaulted)
            {
                Debug.LogError($"Firebase init exception: {task.Exception?.InnerException?.Message}");
                initTcs.TrySetResult(false);
                return;
            }

            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp.LogLevel = LogLevel.Verbose;
                var app = FirebaseApp.DefaultInstance;
                Debug.Log($"Firebase ready | App: {app.Options.AppId} | Project: {app.Options.ProjectId}");

                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
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

    public Task<AuthResult> LoginAnonymously()
    {
#if FB_SDK
        var tcs = new TaskCompletionSource<AuthResult>();
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            try
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    System.Exception ex = task.Exception;
                    Debug.LogError($"LoginAnonymously exception: {ex}");
                    string errorMsg = GetFriendlyErrorMessage(ex);
                    tcs.TrySetResult(new AuthResult { Success = false, ErrorMessage = errorMsg });
                }
                else
                {
                    var result = task.Result;
                    _userId = result.User.UserId;
                    tcs.TrySetResult(new AuthResult { Success = true, UserId = _userId });
                }
            }
            catch (System.Exception cbEx)
            {
                Debug.LogError($"Exception in LoginAnonymously callback: {cbEx}");
                tcs.TrySetResult(new AuthResult { Success = false, ErrorMessage = cbEx.Message });
            }
        });
        return tcs.Task;
#else
        return Task.FromResult(new AuthResult { Success = false, ErrorMessage = "FB_SDK not defined" });
#endif
    }

    public Task<AuthResult> LoginWithEmail(string email, string password)
    {
#if FB_SDK
        var tcs = new TaskCompletionSource<AuthResult>();
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            try
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    System.Exception ex = task.Exception;
                    Debug.LogError($"LoginWithEmail exception: {ex}");
                    string errorMsg = GetFriendlyErrorMessage(ex);
                    tcs.TrySetResult(new AuthResult { Success = false, ErrorMessage = errorMsg });
                }
                else
                {
                    var result = task.Result;
                    _userId = result.User.UserId;
                    tcs.TrySetResult(new AuthResult { Success = true, UserId = _userId });
                }
            }
            catch (System.Exception cbEx)
            {
                Debug.LogError($"Exception in LoginWithEmail callback: {cbEx}");
                tcs.TrySetResult(new AuthResult { Success = false, ErrorMessage = cbEx.Message });
            }
        });
        return tcs.Task;
#else
        return Task.FromResult(new AuthResult { Success = false, ErrorMessage = "FB_SDK not defined" });
#endif
    }

    public Task<AuthResult> RegisterWithEmail(string email, string password)
    {
#if FB_SDK
        var tcs = new TaskCompletionSource<AuthResult>();
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            try
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    System.Exception ex = task.Exception;
                    Debug.LogError($"RegisterWithEmail exception: {ex}");
                    string errorMsg = GetFriendlyErrorMessage(ex);
                    tcs.TrySetResult(new AuthResult { Success = false, ErrorMessage = errorMsg });
                }
                else
                {
                    var result = task.Result;
                    _userId = result.User.UserId;
                    tcs.TrySetResult(new AuthResult { Success = true, UserId = _userId });
                }
            }
            catch (System.Exception cbEx)
            {
                Debug.LogError($"Exception in RegisterWithEmail callback: {cbEx}");
                tcs.TrySetResult(new AuthResult { Success = false, ErrorMessage = cbEx.Message });
            }
        });
        return tcs.Task;
#else
        return Task.FromResult(new AuthResult { Success = false, ErrorMessage = "FB_SDK not defined" });
#endif
    }

#if FB_SDK
    private string GetFriendlyErrorMessage(System.Exception ex)
    {
        if (ex == null) return "Lỗi không xác định.";
        
        System.Exception current = ex;
        FirebaseException fbEx = null;

        while (current != null)
        {
            if (current is FirebaseException fex)
            {
                fbEx = fex;
                break;
            }
            if (current is System.AggregateException aggEx && aggEx.InnerExceptions != null)
            {
                foreach (var inner in aggEx.InnerExceptions)
                {
                    if (inner is FirebaseException innerFex)
                    {
                        fbEx = innerFex;
                        break;
                    }
                }
                if (fbEx != null) break;
            }
            current = current.InnerException;
        }

        if (fbEx != null)
        {
            var errorCode = (AuthError)fbEx.ErrorCode;
            switch (errorCode)
            {
                case AuthError.InvalidEmail:
                    return "Email không hợp lệ.";
                case AuthError.WrongPassword:
                    return "Mật khẩu không chính xác.";
                case AuthError.UserNotFound:
                    return "Tài khoản không tồn tại.";
                case AuthError.EmailAlreadyInUse:
                    return "Email này đã được đăng ký sử dụng.";
                case AuthError.WeakPassword:
                    return "Mật khẩu quá yếu (tối thiểu 6 ký tự).";
                case AuthError.MissingEmail:
                    return "Vui lòng nhập Email.";
                case AuthError.MissingPassword:
                    return "Vui lòng nhập Mật khẩu.";
                case AuthError.UserDisabled:
                    return "Tài khoản này đã bị vô hiệu hóa.";
                case AuthError.NetworkRequestFailed:
                    return "Lỗi kết nối mạng, vui lòng thử lại.";
                default:
                    return $"Lỗi hệ thống ({errorCode}): {fbEx.Message}";
            }
        }

        var baseEx = ex.GetBaseException();
        return baseEx != null ? baseEx.Message : ex.Message;
    }
#endif

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
        var data = JsonUtility.FromJson<GameSaveData>(json);
        if (data == null)
        {
            Debug.LogError("FirebaseCloudSave: Failed to parse save JSON");
            return false;
        }

        var doc = db.Collection("users").Document(userId);
        var fields = DataToDict(data);
        fields["updatedAt"] = Timestamp.GetCurrentTimestamp();
        await doc.SetAsync(fields);
        return true;
#else
        await Task.Yield();
        return false;
#endif
    }

    public async Task<string> LoadPlayerData(string userId)
    {
#if FB_SDK
        var snap = await db.Collection("users").Document(userId).GetSnapshotAsync();
        if (!snap.Exists)
            return null;

        var data = new GameSaveData
        {
            sceneName = snap.TryGetValue("sceneName", out string s) ? s : null,
            saveTime = snap.TryGetValue("saveTime", out string t) ? t : null,
            player = ReadPlayerData(snap),
            enemies = ReadEnemyList(snap),
            killedEnemyIds = ReadStringList(snap, "killedEnemyIds"),
            unlockedFloors = ReadStringList(snap, "unlockedFloors"),
        };

        if (data.player == null)
            return null;

        return JsonUtility.ToJson(data);
#else
        await Task.Yield();
        return null;
#endif
    }

#if FB_SDK
    private Dictionary<string, object> DataToDict(GameSaveData data)
    {
        return new Dictionary<string, object>
        {
            ["sceneName"] = data.sceneName ?? "",
            ["saveTime"] = data.saveTime ?? "",
            ["player"] = PlayerToDict(data.player),
            ["enemies"] = EnemiesToList(data.enemies),
            ["killedEnemyIds"] = data.killedEnemyIds ?? new List<string>(),
            ["unlockedFloors"] = data.unlockedFloors ?? new List<string>(),
        };
    }

    private Dictionary<string, object> PlayerToDict(PlayerSaveData p)
    {
        return new Dictionary<string, object>
        {
            ["posX"] = p.posX,
            ["posY"] = p.posY,
            ["level"] = p.level,
            ["currentExp"] = p.currentExp,
            ["expToNextLevel"] = p.expToNextLevel,
            ["availableStatPoints"] = p.availableStatPoints,
            ["strength"] = p.strength,
            ["dexterity"] = p.dexterity,
            ["vitality"] = p.vitality,
            ["agility"] = p.agility,
            ["endurance"] = p.endurance,
            ["intelligence"] = p.intelligence,
            ["currentHealth"] = p.currentHealth,
            ["maxHealth"] = p.maxHealth,
            ["currentStamina"] = p.currentStamina,
            ["maxStamina"] = p.maxStamina,
            ["currentScene"] = p.currentScene ?? "",
        };
    }

    private List<object> EnemiesToList(List<EnemySaveData> enemies)
    {
        var list = new List<object>();
        if (enemies == null) return list;
        foreach (var e in enemies)
        {
            list.Add(new Dictionary<string, object>
            {
                ["saveId"] = e.saveId ?? "",
                ["isDead"] = e.isDead,
                ["posX"] = e.posX,
                ["posY"] = e.posY,
                ["currentHealth"] = e.currentHealth,
            });
        }
        return list;
    }

    private PlayerSaveData ReadPlayerData(DocumentSnapshot snap)
    {
        var player = new PlayerSaveData();

        if (!snap.ContainsField("player"))
            return null;

        player.posX = snap.GetValue<float>("player.posX");
        player.posY = snap.GetValue<float>("player.posY");
        player.level = snap.GetValue<int>("player.level");
        player.currentExp = snap.GetValue<int>("player.currentExp");
        player.expToNextLevel = snap.GetValue<int>("player.expToNextLevel");
        player.availableStatPoints = snap.GetValue<int>("player.availableStatPoints");
        player.strength = snap.GetValue<int>("player.strength");
        player.dexterity = snap.GetValue<int>("player.dexterity");
        player.vitality = snap.GetValue<int>("player.vitality");
        player.agility = snap.GetValue<int>("player.agility");
        player.endurance = snap.GetValue<int>("player.endurance");
        player.intelligence = snap.GetValue<int>("player.intelligence");
        player.currentHealth = snap.GetValue<int>("player.currentHealth");
        player.maxHealth = snap.GetValue<int>("player.maxHealth");
        player.currentStamina = snap.GetValue<float>("player.currentStamina");
        player.maxStamina = snap.GetValue<float>("player.maxStamina");
        player.currentScene = snap.GetValue<string>("player.currentScene");

        return player;
    }

    private List<EnemySaveData> ReadEnemyList(DocumentSnapshot snap)
    {
        var list = new List<EnemySaveData>();
        if (!snap.ContainsField("enemies"))
            return list;

        var raw = snap.GetValue<List<object>>("enemies");
        if (raw == null) return list;

        foreach (var item in raw)
        {
            if (item is Dictionary<string, object> map)
            {
                list.Add(new EnemySaveData
                {
                    saveId = GetDictStr(map, "saveId"),
                    isDead = GetDictBool(map, "isDead"),
                    posX = GetDictFloat(map, "posX"),
                    posY = GetDictFloat(map, "posY"),
                    currentHealth = GetDictInt(map, "currentHealth"),
                });
            }
        }
        return list;
    }

    private List<string> ReadStringList(DocumentSnapshot snap, string field)
    {
        var list = new List<string>();
        if (!snap.ContainsField(field))
            return list;

        var raw = snap.GetValue<List<object>>(field);
        if (raw == null) return list;

        foreach (var item in raw)
        {
            if (item is string s)
                list.Add(s);
        }
        return list;
    }

    private string GetDictStr(Dictionary<string, object> dict, string key)
        => dict.TryGetValue(key, out var v) && v is string s ? s : "";

    private int GetDictInt(Dictionary<string, object> dict, string key)
        => dict.TryGetValue(key, out var v) && v is long l ? (int)l : 0;

    private float GetDictFloat(Dictionary<string, object> dict, string key)
        => dict.TryGetValue(key, out var v) ? System.Convert.ToSingle(v) : 0f;

    private bool GetDictBool(Dictionary<string, object> dict, string key)
        => dict.TryGetValue(key, out var v) && v is bool b && b;
#endif

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
