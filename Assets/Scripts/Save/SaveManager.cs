using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public static List<string> UnlockedFloors { get; private set; } = new List<string> { "floor_1" };

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
        _ = SaveAsync();
    }

    private async System.Threading.Tasks.Task SaveAsync()
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
        _ = LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        var auth = CloudServiceManager.Instance?.Auth;
        var save = CloudServiceManager.Instance?.Save;
        if (auth == null || save == null)
        {
            FallbackNewGame();
            return;
        }

        string json = await save.LoadPlayerData(auth.UserId);
        if (string.IsNullOrEmpty(json))
        {
            FallbackNewGame();
            return;
        }

        var data = JsonUtility.FromJson<GameSaveData>(json);
        if (data.player == null)
        {
            FallbackNewGame();
            return;
        }

        if (!string.IsNullOrEmpty(data.sceneName) && data.sceneName != SceneManager.GetActiveScene().name)
        {
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;
            _pendingRestoreData = data;
            SceneManager.LoadScene(data.sceneName);
            return;
        }

        ApplyRestore(data);
    }

    private void FallbackNewGame()
    {
        EnemyHealth.KilledEnemyIds.Clear();
        UnlockedFloors.Clear();
        UnlockedFloors.Add("floor_1");
        SceneManager.LoadScene("floor_1");
    }

    private GameSaveData _pendingRestoreData;

    private void OnSceneLoadedForRestore(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
        if (_pendingRestoreData != null)
        {
            ApplyRestore(_pendingRestoreData);
            _pendingRestoreData = null;
        }
    }

    private void ApplyRestore(GameSaveData data)
    {
        EnemyHealth.KilledEnemyIds.Clear();
        if (data.killedEnemyIds != null)
        {
            foreach (var id in data.killedEnemyIds)
                EnemyHealth.KilledEnemyIds.Add(id);
        }

        UnlockedFloors.Clear();
        if (data.unlockedFloors != null && data.unlockedFloors.Count > 0)
            UnlockedFloors.AddRange(data.unlockedFloors);
        else
            UnlockedFloors.Add("floor_1");

        RestorePlayer(data);
        RestoreEnemies(data);
    }

    private void RestorePlayer(GameSaveData data)
    {
        var p = data.player;
        var player = Object.FindFirstObjectByType<PlayerStats>();
        if (player == null) return;

        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.SetCurrentHealth(p.currentHealth);

        player.transform.position = new Vector3(p.posX, p.posY, 0);
    }

    private void RestoreEnemies(GameSaveData data)
    {
        var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            if (string.IsNullOrEmpty(enemy.SaveId)) continue;

            // Destroy enemies that were killed and tracked
            if (EnemyHealth.KilledEnemyIds.Contains(enemy.SaveId))
            {
                Destroy(enemy.gameObject);
                continue;
            }

            // Restore health for enemies that were alive
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
}
