using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;
    public NetworkRunner RunnerPrefab { set { runnerPrefab = value; } }
    [SerializeField] private int maxPlayers = 4;

    private NetworkRunner runner;
    public NetworkRunner Runner => runner;

    public System.Action OnRunnerStarted;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async System.Threading.Tasks.Task LaunchAsSingleplayer(string targetSceneName)
    {
        if (runnerPrefab == null)
        {
            Debug.LogError("GameLauncher: runnerPrefab is null!");
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("GameLauncher: targetSceneName is empty!");
            return;
        }

        if (runner != null && runner.IsRunning)
        {
            Debug.Log("[GameLauncher] Shutting down previous runner...");
            await runner.Shutdown();
            Destroy(runner.gameObject);
            runner = null;
        }

        Debug.Log($"[GameLauncher] Loading scene: {targetSceneName}");
        DontDestroyOnLoad(gameObject);

        string scenePath = $"Assets/Scenes/{targetSceneName}.unity";
        if (SceneUtility.GetBuildIndexByScenePath(targetSceneName) >= 0)
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else if (SceneUtility.GetBuildIndexByScenePath(scenePath) >= 0)
        {
            SceneManager.LoadScene(scenePath);
        }
        else
        {
            Debug.Log($"[GameLauncher] Scene not in build profile, trying full path: {scenePath}");
            SceneManager.LoadScene(scenePath);
        }

        await System.Threading.Tasks.Task.Yield();

        Debug.Log("[GameLauncher] Creating NetworkRunner...");
        runner = Instantiate(runnerPrefab);
        runner.name = "Host";
        DontDestroyOnLoad(runner);

        Debug.Log("[GameLauncher] Starting Fusion (GameMode.Single)...");
        var args = new StartGameArgs
        {
            GameMode = GameMode.Single,
        };

        var result = await runner.StartGame(args);

        if (result.Ok == false)
        {
            Debug.LogError($"[GameLauncher] Failed: {result.ShutdownReason}");
            Destroy(runner.gameObject);
            runner = null;
            return;
        }

        Debug.Log("[GameLauncher] Fusion started. PlayerSpawner.OnPlayerJoined will handle spawn.");
    }

    public async System.Threading.Tasks.Task LaunchAsHost(string targetSceneName, string sessionName)
    {
        if (runnerPrefab == null)
        {
            Debug.LogError("GameLauncher: runnerPrefab is null!");
            return;
        }

        if (!GameSessionData.TryValidateSessionName(sessionName, out string validatedSessionName, out string validationError))
        {
            Debug.LogWarning($"[GameLauncher] Invalid session name: {validationError}");
            return;
        }

        if (runner != null && runner.IsRunning)
        {
            Debug.Log("[GameLauncher] Shutting down previous runner...");
            await runner.Shutdown();
            Destroy(runner.gameObject);
            runner = null;
        }

        DontDestroyOnLoad(gameObject);

        runner = Instantiate(runnerPrefab);
        runner.name = "Host";
        DontDestroyOnLoad(runner);

        InitializeLobby(targetSceneName);

        Debug.Log($"[GameLauncher] Starting Fusion as Host, session: {validatedSessionName}");
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var args = new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = validatedSessionName,
            PlayerCount = maxPlayers,
            Scene = currentScene.IsValid() ? SceneRef.FromIndex(currentScene.buildIndex) : default,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
        };

        StartGameResult result;
        try
        {
            result = await runner.StartGame(args);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameLauncher] Host threw exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            Destroy(runner.gameObject);
            runner = null;
            return;
        }

        if (result.Ok == false)
        {
            Debug.LogError($"[GameLauncher] Host failed: {result.ShutdownReason}");
            if (runner != null)
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                Debug.LogError($"[GameLauncher] Stack: {stackTrace}");
                Destroy(runner.gameObject);
            }
            runner = null;
            return;
        }

        OnRunnerStarted?.Invoke();
    }

    public async System.Threading.Tasks.Task LaunchAsClient(string sessionName)
    {
        if (runnerPrefab == null)
        {
            Debug.LogError("GameLauncher: runnerPrefab is null!");
            return;
        }

        if (!GameSessionData.TryValidateSessionName(sessionName, out string validatedSessionName, out string validationError))
        {
            Debug.LogWarning($"[GameLauncher] Invalid session name: {validationError}");
            return;
        }

        if (runner != null && runner.IsRunning)
        {
            Debug.Log("[GameLauncher] Shutting down previous runner...");
            await runner.Shutdown();
            Destroy(runner.gameObject);
            runner = null;
        }

        DontDestroyOnLoad(gameObject);

        runner = Instantiate(runnerPrefab);
        runner.name = "Client";
        DontDestroyOnLoad(runner);

        InitializeLobby(null);

        Debug.Log($"[GameLauncher] Joining session: {validatedSessionName}");
        var args = new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = validatedSessionName,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
        };

        var result = await runner.StartGame(args);

        if (result.Ok == false)
        {
            Debug.LogError($"[GameLauncher] Client failed: {result.ShutdownReason}");
            Destroy(runner.gameObject);
            runner = null;
            return;
        }

        OnRunnerStarted?.Invoke();
    }

    public async void LoadGameScene(string sceneName)
    {
        if (runner == null || !runner.IsRunning) return;

        int buildIndex = ResolveBuildIndex(sceneName);
        if (buildIndex < 0)
        {
            Debug.LogError($"[GameLauncher] Scene {sceneName} not in build settings!");
            return;
        }

        Debug.Log($"[GameLauncher] Loading game scene: {sceneName} (index {buildIndex})");
        await runner.LoadScene(SceneRef.FromIndex(buildIndex));
    }

    private static int ResolveBuildIndex(string sceneName)
    {
        int index = SceneUtility.GetBuildIndexByScenePath(sceneName);
        if (index >= 0) return index;

        index = SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{sceneName}.unity");
        if (index >= 0) return index;

        index = SceneUtility.GetBuildIndexByScenePath("floor1");
        if (index >= 0) return index;

        return SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/floor1.unity");
    }

    private void InitializeLobby(string targetSceneName)
    {
        var lobby = FindFirstObjectByType<NetworkLobby>();
        if (lobby == null)
        {
            Debug.LogWarning("[GameLauncher] NetworkLobby scene object not found.");
            return;
        }

        lobby.Init(this, targetSceneName);
    }

    private void OnDestroy()
    {
        if (runner != null)
        {
            if (runner.IsRunning)
            {
                Debug.Log("[GameLauncher] Shutting down runner.");
                runner.Shutdown();
            }
            if (runner.gameObject != null)
                Destroy(runner.gameObject);
            runner = null;
        }
    }
}
