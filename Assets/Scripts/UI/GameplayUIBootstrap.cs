using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void EnsurePlayerHud()
    {
        if (!createPlayerHud)
        {
            return;
        }

        if (currentPlayer != null && hudController != null)
        {
            return;
        }

        playerScanTimer -= Time.deltaTime;
        if (playerScanTimer > 0f)
        {
            return;
        }

        playerScanTimer = 0.25f;
        currentPlayer = FindFirstObjectByType<PlayerStats>();
        if (currentPlayer == null)
        {
            return;
        }

        if (hudController == null)
        {
            hudController = FindFirstObjectByType<GameplayHUDController>();
        }

        if (hudController == null)
        {
            hudController = GameplayHUDController.CreateRuntimeHud();
        }

        hudController.SetPlayer(currentPlayer);
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
}
