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
        if (runner != null && runner.IsRunning)
            return;

        if (runnerPrefab == null)
        {
            Debug.LogError("GameLauncher: runnerPrefab is null!");
            return;
        }

        Debug.Log($"[GameLauncher] Loading scene: {targetSceneName}");
        DontDestroyOnLoad(gameObject);

        SceneManager.LoadScene(targetSceneName);

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
        if (runner != null && runner.IsRunning)
            return;

        if (runnerPrefab == null)
        {
            Debug.LogError("GameLauncher: runnerPrefab is null!");
            return;
        }

        DontDestroyOnLoad(gameObject);

        runner = Instantiate(runnerPrefab);
        runner.name = "Host";
        DontDestroyOnLoad(runner);

        int buildIndex = SceneUtility.GetBuildIndexByScenePath(targetSceneName);
        if (buildIndex < 0)
            buildIndex = SceneUtility.GetBuildIndexByScenePath("floor1");

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
        if (runner != null && runner.IsRunning)
            return;

        if (runnerPrefab == null)
        {
            Debug.LogError("GameLauncher: runnerPrefab is null!");
            return;
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

    private void OnDestroy()
    {
        if (runner != null && runner.IsRunning)
        {
            Debug.Log("[GameLauncher] Shutting down runner.");
            runner.Shutdown();
        }
    }
}
