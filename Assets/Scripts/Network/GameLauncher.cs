using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private int maxPlayers = 4;

    private NetworkRunner runner;

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

        int buildIndex = ResolveBuildIndex(targetSceneName);

        Debug.Log($"[GameLauncher] Starting Fusion as Host, session: {sessionName}, scene index: {buildIndex}");
        var args = new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            PlayerCount = maxPlayers,
            Scene = SceneRef.FromIndex(buildIndex),
        };

        var result = await runner.StartGame(args);

        if (result.Ok == false)
        {
            Debug.LogError($"[GameLauncher] Host failed: {result.ShutdownReason}");
            Destroy(runner.gameObject);
            runner = null;
        }
    }

    public async System.Threading.Tasks.Task LaunchAsClient(string sessionName)
    {
        if (runnerPrefab == null)
        {
            Debug.LogError("GameLauncher: runnerPrefab is null!");
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

        Debug.Log($"[GameLauncher] Joining session: {sessionName}");
        var args = new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
        };

        var result = await runner.StartGame(args);

        if (result.Ok == false)
        {
            Debug.LogError($"[GameLauncher] Client failed: {result.ShutdownReason}");
            Destroy(runner.gameObject);
            runner = null;
        }
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

    private void OnDestroy()
    {
        if (runner != null && runner.IsRunning)
        {
            Debug.Log("[GameLauncher] Shutting down runner.");
            runner.Shutdown();
        }
    }
}
