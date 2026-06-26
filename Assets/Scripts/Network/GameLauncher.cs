using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private string sessionName = "AbyssFrontier";

    private NetworkRunner runner;

    private void OnGUI()
    {
        if (runner != null && runner.IsRunning)
        {
            GUILayout.Label($"Connected as {runner.GameMode} | Players: {runner.ActivePlayers.Count}");
            if (GUILayout.Button("Disconnect"))
            {
                runner.Shutdown();
                Destroy(runner.gameObject);
                runner = null;
            }
            return;
        }

        GUILayout.BeginArea(new Rect(Screen.width / 2f - 100, Screen.height / 2f - 60, 200, 120));
        GUILayout.Label("Abyss Frontier - Network");

        if (GUILayout.Button("Host Game"))
            _ = StartConnection(GameMode.Host);

        if (GUILayout.Button("Join Game"))
            _ = StartConnection(GameMode.Client);

        GUILayout.EndArea();
    }

    private async System.Threading.Tasks.Task StartConnection(GameMode mode)
    {
        if (runnerPrefab == null)
        {
            Debug.LogError("GameLauncher: runnerPrefab is null!");
            return;
        }

        runner = Instantiate(runnerPrefab);
        runner.name = mode.ToString();
        DontDestroyOnLoad(runner);

        var sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        if (sceneRef.IsValid == false)
        {
            Debug.LogError("Active scene not in Build Settings!");
            return;
        }

        var sceneManager = runner.GetComponent<INetworkSceneManager>();
        if (sceneManager == null)
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var args = new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = sceneRef,
            SceneManager = sceneManager,
            ObjectProvider = runner.GetComponent<INetworkObjectProvider>(),
        };

        var result = await runner.StartGame(args);

        if (result.Ok == false)
        {
            Debug.LogError($"Failed: {result.ShutdownReason}");
            Destroy(runner.gameObject);
            runner = null;
        }
    }

    private void OnDestroy()
    {
        if (runner != null && runner.IsRunning)
            runner.Shutdown();
    }
}
