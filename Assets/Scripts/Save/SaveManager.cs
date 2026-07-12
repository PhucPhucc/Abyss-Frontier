using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public static List<string> UnlockedFloors { get; private set; } = new List<string> { "floor1", "floor2", "floor3", "floor4", "floor5" };

    private static readonly string[] AllFloorKeys = { "floor1", "floor2", "floor3", "floor4", "floor5" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInit()
    {
        if (Instance == null)
        {
            var go = new GameObject("SaveManager");
            go.AddComponent<SaveManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private static string SaveKeyForScene(string sceneName)
    {
        string uid = CloudServiceManager.Instance?.Auth?.UserId;
        return uid != null ? $"SaveData_{uid}_{sceneName}" : $"SaveData_Local_{sceneName}";
    }

    public static bool IsFloorUnlocked(string sceneName)
    {
        return UnlockedFloors.Contains(sceneName);
    }

    public static void UnlockFloor(string sceneName)
    {
        if (!UnlockedFloors.Contains(sceneName))
            UnlockedFloors.Add(sceneName);
    }

    private static bool _hasAnySavedData;

    public bool HasSavedGame
    {
        get
        {
            if (_hasAnySavedData) return true;
            foreach (var floor in AllFloorKeys)
            {
                if (PlayerPrefs.HasKey(SaveKeyForScene(floor)))
                    return true;
            }
            if (HasOldFormatSave())
                return true;
            return false;
        }
    }

    public static bool HasSaveForMap(string sceneName)
    {
        if (PlayerPrefs.HasKey(SaveKeyForScene(sceneName)))
            return true;
        return HasOldFormatSaveForScene(sceneName);
    }

    private static bool HasOldFormatSave()
    {
        string uid = CloudServiceManager.Instance?.Auth?.UserId;
        string oldKey = uid != null ? "DummySaveData_" + uid : "DummySaveData_Local";
        return PlayerPrefs.HasKey(oldKey);
    }

    private static bool HasOldFormatSaveForScene(string sceneName)
    {
        string uid = CloudServiceManager.Instance?.Auth?.UserId;
        string oldKey = uid != null ? "DummySaveData_" + uid : "DummySaveData_Local";
        if (!PlayerPrefs.HasKey(oldKey))
            return false;
        string json = PlayerPrefs.GetString(oldKey);
        if (string.IsNullOrEmpty(json))
            return false;
        var data = JsonUtility.FromJson<GameSaveData>(json);
        return data != null && data.sceneName == sceneName;
    }

    public static List<string> GetSavedMaps()
    {
        var result = new List<string>();
        foreach (var floor in AllFloorKeys)
        {
            if (PlayerPrefs.HasKey(SaveKeyForScene(floor)))
                result.Add(floor);
        }
        string uid = CloudServiceManager.Instance?.Auth?.UserId;
        string oldKey = uid != null ? "DummySaveData_" + uid : "DummySaveData_Local";
        if (PlayerPrefs.HasKey(oldKey))
        {
            string json = PlayerPrefs.GetString(oldKey);
            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonUtility.FromJson<GameSaveData>(json);
                if (data != null && !string.IsNullOrEmpty(data.sceneName) && !result.Contains(data.sceneName))
                    result.Add(data.sceneName);
            }
        }
        return result;
    }

    public static void ClearSaveForMap(string sceneName)
    {
        string key = SaveKeyForScene(sceneName);
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.DeleteKey(key);
        string uid = CloudServiceManager.Instance?.Auth?.UserId;
        string oldKey = uid != null ? "DummySaveData_" + uid : "DummySaveData_Local";
        if (PlayerPrefs.HasKey(oldKey))
        {
            string json = PlayerPrefs.GetString(oldKey);
            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonUtility.FromJson<GameSaveData>(json);
                if (data != null && data.sceneName == sceneName)
                    PlayerPrefs.DeleteKey(oldKey);
            }
        }
        PlayerPrefs.Save();
    }

    public static void ClearSavedDataFlag()
    {
        _hasAnySavedData = false;
        foreach (var floor in AllFloorKeys)
        {
            string key = SaveKeyForScene(floor);
            if (PlayerPrefs.HasKey(key))
                PlayerPrefs.DeleteKey(key);
        }
        string uid = CloudServiceManager.Instance?.Auth?.UserId;
        string oldKey = uid != null ? "DummySaveData_" + uid : "DummySaveData_Local";
        if (PlayerPrefs.HasKey(oldKey))
            PlayerPrefs.DeleteKey(oldKey);
        PlayerPrefs.Save();
    }

    private static void MigrateOldSaveIfNeeded()
    {
        string uid = CloudServiceManager.Instance?.Auth?.UserId;
        string oldKey = uid != null ? "DummySaveData_" + uid : "DummySaveData_Local";
        if (!PlayerPrefs.HasKey(oldKey))
            return;

        string json = PlayerPrefs.GetString(oldKey);
        if (string.IsNullOrEmpty(json))
        {
            PlayerPrefs.DeleteKey(oldKey);
            PlayerPrefs.Save();
            return;
        }

        var data = JsonUtility.FromJson<GameSaveData>(json);
        if (data == null || string.IsNullOrEmpty(data.sceneName))
        {
            PlayerPrefs.DeleteKey(oldKey);
            PlayerPrefs.Save();
            return;
        }

        string newKey = SaveKeyForScene(data.sceneName);
        if (!PlayerPrefs.HasKey(newKey))
        {
            PlayerPrefs.SetString(newKey, json);
            Debug.Log($"[SaveManager] Migrated old save to {newKey}");
        }

        PlayerPrefs.DeleteKey(oldKey);
        PlayerPrefs.Save();
    }

    public void SaveGame()
    {
        if (GameSessionData.IsMultiplayer) return;
        SaveInternal();
    }

    public async System.Threading.Tasks.Task SaveGameAsync()
    {
        if (GameSessionData.IsMultiplayer) return;
        SaveInternal();
        if (CloudServiceManager.Instance?.Save != null && CloudServiceManager.Instance?.Auth?.UserId != null)
        {
            await CloudSaveAsync();
        }
        await System.Threading.Tasks.Task.CompletedTask;
    }

    private void SaveInternal()
    {
        var data = BuildSaveData();
        string json = JsonUtility.ToJson(data);
        string sceneName = SceneManager.GetActiveScene().name;

        PlayerPrefs.SetString(SaveKeyForScene(sceneName), json);
        PlayerPrefs.Save();
        _hasAnySavedData = true;
        Debug.Log($"[SaveManager] Game saved for '{sceneName}'");
    }

    private async System.Threading.Tasks.Task CloudSaveAsync()
    {
        var data = BuildSaveData();
        string json = JsonUtility.ToJson(data);

        var success = await CloudServiceManager.Instance.Save.SavePlayerData(
            CloudServiceManager.Instance.Auth.UserId, json);

        if (success)
            Debug.Log("[SaveManager] Cloud save successful.");
        else
            Debug.LogWarning("[SaveManager] Cloud save failed, local save still available.");
    }

    private PlayerStats FindLocalPlayerStats()
    {
        var allPlayers = Object.FindObjectsByType<PlayerStats>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            if (p != null && p.TryGetComponent<NetworkObject>(out var netObj) && netObj.HasInputAuthority)
            {
                return p;
            }
        }
        return Object.FindFirstObjectByType<PlayerStats>();
    }

    private GameSaveData BuildSaveData()
    {
        var data = new GameSaveData();
        data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.sceneName = SceneManager.GetActiveScene().name;

        var player = FindLocalPlayerStats();
        if (player != null)
        {
            data.player = new PlayerSaveData
            {
                posX = player.transform.position.x,
                posY = player.transform.position.y,
                level = player.Level,
                currentExp = player.CurrentExp,
                expToNextLevel = player.ExpToNextLevel,
                availableStatPoints = player.AvailableStatPoints,
                strength = player.Strength,
                dexterity = player.Dexterity,
                vitality = player.Vitality,
                agility = player.Agility,
                endurance = player.Endurance,
                intelligence = player.Intelligence,
                currentHealth = player.CurrentHealth,
                maxHealth = player.MaxHealth,
                currentStamina = player.CurrentStamina,
                maxStamina = player.MaxStamina,
                currentScene = data.sceneName,
            };
        }

        var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        data.enemies = new List<EnemySaveData>();
        foreach (var e in enemies)
        {
            data.enemies.Add(new EnemySaveData
            {
                saveId = e.SaveId,
                isDead = e.IsDead,
                posX = e.transform.position.x,
                posY = e.transform.position.y,
                currentHealth = e.CurrentHealth,
            });
        }

        data.killedEnemyIds = new List<string>(EnemyHealth.KilledEnemyIds);
        data.unlockedFloors = new List<string>(UnlockedFloors);
        data.characterIndex = GameSessionData.SelectedCharacterIndex;
        return data;
    }

    public void ContinueGame()
    {
        var savedMaps = GetSavedMaps();
        if (savedMaps.Count == 0)
        {
            FallbackNewGame();
            return;
        }
        _ = ContinueAsync(savedMaps[0]);
    }

    public void ContinueGame(string sceneName)
    {
        _ = ContinueAsync(sceneName);
    }

    private async System.Threading.Tasks.Task ContinueAsync(string sceneName)
    {
        MigrateOldSaveIfNeeded();

        string json = TryLoadLocalSave(sceneName);
        var auth = CloudServiceManager.Instance?.Auth;
        var save = CloudServiceManager.Instance?.Save;

        if (string.IsNullOrEmpty(json) && auth != null && save != null)
        {
            json = await save.LoadPlayerData(auth.UserId);
        }

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("[SaveManager] No save data for " + sceneName);
            FallbackNewGame();
            return;
        }

        var data = JsonUtility.FromJson<GameSaveData>(json);
        if (data.player == null)
        {
            Debug.Log("[SaveManager] Invalid save data (player=null) → FallbackNewGame");
            FallbackNewGame();
            return;
        }

        string targetScene = data.sceneName;
        if (string.IsNullOrEmpty(targetScene))
            targetScene = "floor1";

        GameSessionData.SelectedCharacterIndex = data.characterIndex;
        _pendingRestoreData = data;

        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
        {
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;
            await launcher.LaunchAsSingleplayer(targetScene);
        }
        else
        {
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;
            SceneManager.LoadScene(targetScene);
        }
    }

    private static string TryLoadLocalSave(string sceneName)
    {
        string key = SaveKeyForScene(sceneName);
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            if (!string.IsNullOrEmpty(json))
            {
                Debug.Log($"[SaveManager] Loaded save for '{sceneName}'");
                return json;
            }
        }
        return null;
    }

    private void FallbackNewGame()
    {
        _hasAnySavedData = false;
        Debug.Log("[SaveManager] FallbackNewGame");
        EnemyHealth.KilledEnemyIds.Clear();
        UnlockedFloors.Clear();
        UnlockedFloors.AddRange(new[] { "floor1", "floor2", "floor3", "floor4", "floor5" });
        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
            _ = launcher.LaunchAsSingleplayer("floor1");
        else
            SceneManager.LoadScene("floor1");
    }

    private GameSaveData _pendingRestoreData;

    private void OnSceneLoadedForRestore(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
        if (GameSessionData.IsMultiplayer)
        {
            _pendingRestoreData = null;
            return;
        }
        if (_pendingRestoreData == null) return;

        var data = _pendingRestoreData;
        _pendingRestoreData = null;

        EnemyHealth.KilledEnemyIds.Clear();
        if (data.killedEnemyIds != null)
            foreach (var id in data.killedEnemyIds)
                EnemyHealth.KilledEnemyIds.Add(id);

        UnlockedFloors.Clear();
        if (data.unlockedFloors != null && data.unlockedFloors.Count > 0)
            UnlockedFloors.AddRange(data.unlockedFloors);
        else
            UnlockedFloors.AddRange(new[] { "floor1", "floor2", "floor3", "floor4", "floor5" });

        RestoreEnemies(data);
        StartCoroutine(WaitForPlayerAndRestore(data));
    }

    private void RestoreEnemies(GameSaveData data)
    {
        var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            if (string.IsNullOrEmpty(enemy.SaveId)) continue;

            if (EnemyHealth.KilledEnemyIds.Contains(enemy.SaveId))
            {
                Destroy(enemy.gameObject);
                continue;
            }

            if (data.enemies != null)
            {
                foreach (var saveEnemy in data.enemies)
                {
                    if (enemy.SaveId == saveEnemy.saveId)
                    {
                        if (saveEnemy.isDead)
                            Destroy(enemy.gameObject);
                        else
                            enemy.SetCurrentHealth(saveEnemy.currentHealth);
                        break;
                    }
                }
            }
        }
    }

    private IEnumerator WaitForPlayerAndRestore(GameSaveData data)
    {
        PlayerStats player = null;
        while (player == null)
        {
            player = FindLocalPlayerStats();
            yield return null;
        }

        var p = data.player;
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.SetCurrentHealth(p.currentHealth);

        player.transform.position = new Vector3(p.posX, p.posY, 0);
        Debug.Log($"[SaveManager] Player restored: HP={p.currentHealth}, pos=({p.posX},{p.posY})");
    }
}
