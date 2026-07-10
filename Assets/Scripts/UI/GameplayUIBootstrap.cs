using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using Fusion;

[DisallowMultipleComponent]
public class GameplayUIBootstrap : MonoBehaviour
{
    [Header("Runtime UI")]
    [SerializeField] private bool createPlayerHud = true;
    [SerializeField] private bool attachEnemyHealthBars = true;
    [SerializeField] private float enemyScanInterval = 0.75f;

    private GameplayHUDController hudController;
    private PlayerStats currentPlayer;
    private float enemyScanTimer;
    private float playerScanTimer;
    private int refreshRetries;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeForRuntime()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureSceneBootstrap();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureSceneBootstrap();
    }

    private static void EnsureSceneBootstrap()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene == "Scene_Menu" || activeScene == "Authenticaion" || activeScene == "Scene-Server")
        {
            return;
        }

        if (FindFirstObjectByType<GameplayUIBootstrap>() != null)
        {
            return;
        }

        new GameObject("Gameplay UI Bootstrap", typeof(GameplayUIBootstrap));
    }

    private void Update()
    {
        EnsurePlayerHud();
        TickEnemyHealthBars();
    }

    private void Awake()
    {
        EnsureEventSystem();
    }

    private void EnsurePlayerHud()
    {
        if (FindFirstObjectByType<PauseManager>() == null)
        {
            var prefab = Resources.Load<GameObject>("UI/PauseMenu");
            if (prefab != null)
            {
                var go = Instantiate(prefab);
                go.name = "[PauseMenu]";
            }
        }

        if (!createPlayerHud)
        {
            return;
        }

        // Check if current player is still valid and has input authority if in multiplayer
        bool isCurrentPlayerValid = currentPlayer != null;
        if (isCurrentPlayerValid)
        {
            var runner = FindFirstObjectByType<NetworkRunner>();
            bool isMultiplayer = runner != null && runner.IsRunning && runner.GameMode != GameMode.Single;
            if (isMultiplayer)
            {
                if (currentPlayer.TryGetComponent<NetworkObject>(out var netObj))
                {
                    if (!netObj.HasInputAuthority)
                    {
                        isCurrentPlayerValid = false;
                    }
                }
                else
                {
                    isCurrentPlayerValid = false;
                }
            }
        }

        if (isCurrentPlayerValid && hudController != null)
        {
            if (refreshRetries > 0)
            {
                refreshRetries--;
                hudController.SetPlayer(currentPlayer);
            }
            return;
        }

        playerScanTimer -= Time.deltaTime;
        if (playerScanTimer > 0f)
        {
            return;
        }

        playerScanTimer = 0.25f;

        var currentRunner = FindFirstObjectByType<NetworkRunner>();
        bool activeMultiplayer = currentRunner != null && currentRunner.IsRunning && currentRunner.GameMode != GameMode.Single;

        PlayerStats targetPlayer = null;
        var allPlayers = FindObjectsByType<PlayerStats>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var p in allPlayers)
        {
            if (p != null)
            {
                if (p.TryGetComponent<NetworkObject>(out var netObj))
                {
                    if (netObj.HasInputAuthority)
                    {
                        targetPlayer = p;
                        break;
                    }
                }
            }
        }

        // If not in multiplayer and no player with Input Authority is found, fall back to the first one
        if (targetPlayer == null && !activeMultiplayer)
        {
            if (allPlayers.Length > 0)
            {
                targetPlayer = allPlayers[0];
            }
        }

        if (targetPlayer == null)
        {
            currentPlayer = null;
            return;
        }

        currentPlayer = targetPlayer;

        if (hudController == null)
        {
            hudController = FindFirstObjectByType<GameplayHUDController>();
        }

        if (hudController == null)
        {
            hudController = GameplayHUDController.CreateRuntimeHud();
        }

        hudController.SetPlayer(currentPlayer);
        refreshRetries = 10;
    }

    private void TickEnemyHealthBars()
    {
        if (!attachEnemyHealthBars)
        {
            return;
        }

        enemyScanTimer -= Time.deltaTime;
        if (enemyScanTimer > 0f)
        {
            return;
        }

        enemyScanTimer = enemyScanInterval;
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            EnemyHealthBarUI healthBar = enemy.GetComponentInChildren<EnemyHealthBarUI>(true);
            if (healthBar == null)
            {
                healthBar = enemy.gameObject.AddComponent<EnemyHealthBarUI>();
            }

            healthBar.SetTarget(enemy);
        }
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        var module = es.GetComponent<InputSystemUIInputModule>();
        if (module.actionsAsset == null)
            module.AssignDefaultActions();
    }
}
