using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public static List<string> UnlockedFloors { get; private set; } = new List<string> { "floor1" };

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

    public static bool IsFloorUnlocked(string sceneName)
    {
        return UnlockedFloors.Contains(sceneName);
    }

    public static void UnlockFloor(string sceneName)
    {
        if (!UnlockedFloors.Contains(sceneName))
            UnlockedFloors.Add(sceneName);
    }

    public bool HasSavedGame
    {
        get
        {
            string uid = CloudServiceManager.Instance?.Auth?.UserId;
            return uid != null && PlayerPrefs.HasKey("DummySaveData_" + uid);
        }
    }

    public void SaveGame()
    {
        if (CloudServiceManager.Instance?.Save == null) return;
        _ = SaveInternalAsync();
    }

    public async System.Threading.Tasks.Task SaveGameAsync()
    {
        if (CloudServiceManager.Instance?.Save == null) return;
        await SaveInternalAsync();
    }

    private async System.Threading.Tasks.Task SaveInternalAsync()
    {
        var data = new GameSaveData();
        data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.sceneName = SceneManager.GetActiveScene().name;

        var player = Object.FindFirstObjectByType<PlayerStats>();
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

        string json = JsonUtility.ToJson(data);
        var success = await CloudServiceManager.Instance.Save.SavePlayerData(
            CloudServiceManager.Instance.Auth.UserId, json);

        Debug.Log(success ? "Game saved!" : "Save failed!");
    }

    public void ContinueGame()
    {
        _ = ContinueAsync();
    }

    private async System.Threading.Tasks.Task ContinueAsync()
    {
        var auth = CloudServiceManager.Instance?.Auth;
        var save = CloudServiceManager.Instance?.Save;
        Debug.Log($"[SaveManager] ContinueAsync: auth={auth != null}, save={save != null}, userId={auth?.UserId}");
        if (auth == null || save == null)
        {
            Debug.Log("[SaveManager] Auth or Save null → FallbackNewGame");
            FallbackNewGame();
            return;
        }

        string json = await save.LoadPlayerData(auth.UserId);
        Debug.Log($"[SaveManager] LoadPlayerData: json={(json != null ? json.Substring(0, Mathf.Min(80, json.Length)) : "null")}");
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("[SaveManager] No saved data → FallbackNewGame");
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

        string sceneName = data.sceneName;
        if (string.IsNullOrEmpty(sceneName))
            sceneName = "floor1";

        _pendingRestoreData = data;

        var launcher = FindFirstObjectByType<GameLauncher>();
        Debug.Log($"[SaveManager] GameLauncher found: {launcher != null}");
        if (launcher != null)
        {
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;
            Debug.Log($"[SaveManager] sceneName from save = {sceneName}");
            await launcher.LaunchAsSingleplayer(sceneName);
        }
        else
        {
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;
            SceneManager.LoadScene(sceneName);
        }
    }

    private void FallbackNewGame()
    {
        Debug.Log("[SaveManager] FallbackNewGame");
        EnemyHealth.KilledEnemyIds.Clear();
        UnlockedFloors.Clear();
        UnlockedFloors.Add("floor1");
        var launcher = FindFirstObjectByType<GameLauncher>();
        Debug.Log($"[SaveManager] GameLauncher found: {launcher != null}");
        if (launcher != null)
            _ = launcher.LaunchAsSingleplayer("floor1");
        else
            SceneManager.LoadScene("floor1");
    }

    private GameSaveData _pendingRestoreData;

    private void OnSceneLoadedForRestore(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
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
            UnlockedFloors.Add("floor1");

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
            player = Object.FindFirstObjectByType<PlayerStats>();
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
