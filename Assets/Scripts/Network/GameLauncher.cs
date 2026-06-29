using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private string sessionName = "AbyssFrontier";
    [SerializeField] private int maxPlayers = 4;

    private NetworkRunner runner;

    public async System.Threading.Tasks.Task LaunchAsHost(string targetSceneName)
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

    private void OnDestroy()
    {
        if (runner != null && runner.IsRunning)
        {
            Debug.Log("[GameLauncher] Shutting down runner.");
            runner.Shutdown();
        }
    }
}
