using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner runnerPrefab;
    public NetworkRunner RunnerPrefab { set { runnerPrefab = value; } }
    [SerializeField] private int maxPlayers = 4;

    private NetworkRunner runner;
    public NetworkRunner Runner => runner;

    public Action OnRunnerStarted;
    public Action<List<SessionInfo>> OnSessionListUpdated;
    public Action<string> OnDisconnected;
    public Action<string> OnConnectFailed;

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
        runner.AddCallbacks(this);

        InitializeLobby(targetSceneName);

        Debug.Log($"[GameLauncher] Starting Fusion as Host, session: {validatedSessionName}");
        var currentScene = SceneManager.GetActiveScene();
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
        catch (Exception ex)
        {
            Debug.LogError($"[GameLauncher] Host threw exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            OnConnectFailed?.Invoke($"Lỗi kết nối: {ex.Message}");
            Destroy(runner.gameObject);
            runner = null;
            return;
        }

        if (result.Ok == false)
        {
            Debug.LogError($"[GameLauncher] Host failed: {result.ShutdownReason}");
            OnConnectFailed?.Invoke($"Không thể tạo phòng: {result.ShutdownReason}");
            if (runner != null)
            {
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
        runner.AddCallbacks(this);

        InitializeLobby(null);

        Debug.Log($"[GameLauncher] Joining session: {validatedSessionName}");
        var args = new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = validatedSessionName,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
        };

        StartGameResult result;
        try
        {
            result = await runner.StartGame(args);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameLauncher] Client threw exception: {ex.Message}");
            OnConnectFailed?.Invoke($"Không thể tham gia phòng: {ex.Message}");
            Destroy(runner.gameObject);
            runner = null;
            return;
        }

        if (result.Ok == false)
        {
            Debug.LogError($"[GameLauncher] Client failed: {result.ShutdownReason}");
            OnConnectFailed?.Invoke($"Không thể tham gia phòng: {result.ShutdownReason}");
            Destroy(runner.gameObject);
            runner = null;
            return;
        }

        OnRunnerStarted?.Invoke();
    }

    public async void JoinSessionLobby()
    {
        if (runner == null || !runner.IsRunning)
        {
            if (runnerPrefab == null)
            {
                Debug.LogError("[GameLauncher] runnerPrefab is null!");
                return;
            }

            DontDestroyOnLoad(gameObject);

            runner = Instantiate(runnerPrefab);
            runner.name = "LobbyBrowser";
            DontDestroyOnLoad(runner);
            runner.AddCallbacks(this);

            var args = new StartGameArgs
            {
                GameMode = GameMode.Client,
            };

            var result = await runner.StartGame(args);
            if (result.Ok == false)
            {
                Debug.LogError($"[GameLauncher] Lobby browser failed: {result.ShutdownReason}");
                OnConnectFailed?.Invoke("Không thể kết nối danh sách phòng.");
                Destroy(runner.gameObject);
                runner = null;
                return;
            }
        }

        Debug.Log("[GameLauncher] Joining session lobby...");
        await runner.JoinSessionLobby(SessionLobby.ClientServer, "lobby");
    }

    public async void ShutdownRunner()
    {
        if (runner != null && runner.IsRunning)
        {
            await runner.Shutdown();
        }
        if (runner != null)
        {
            Destroy(runner.gameObject);
            runner = null;
        }
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

    // ── INetworkRunnerCallbacks ──

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"[GameLauncher] Session list updated: {sessionList.Count} sessions");
        OnSessionListUpdated?.Invoke(sessionList);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        string msg = reason switch
        {
            NetDisconnectReason.Timeout => "Mất kết nối với server (timeout).",
            NetDisconnectReason.ServerConnectionRefused => "Server từ chối kết nối.",
            NetDisconnectReason.GameIsFull => "Phòng đã đầy.",
            _ => $"Đã ngắt kết nối: {reason}"
        };
        Debug.LogWarning($"[GameLauncher] Disconnected: {reason}");
        OnDisconnected?.Invoke(msg);
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        string msg = reason switch
        {
            NetConnectFailedReason.ServerFull => "Phòng đã đầy.",
            NetConnectFailedReason.NetworkError => "Lỗi mạng, vui lòng thử lại.",
            NetConnectFailedReason.IncorrectProtocol => "Phiên bản không tương thích.",
            _ => $"Không thể kết nối: {reason}"
        };
        Debug.LogWarning($"[GameLauncher] Connect failed: {reason}");
        OnConnectFailed?.Invoke(msg);
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
