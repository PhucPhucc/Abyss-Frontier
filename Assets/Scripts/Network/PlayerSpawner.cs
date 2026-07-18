using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkObject[] playerPrefabs;
    [SerializeField] private string spawnPointTag = "SpawnPoint";

    private NetworkRunner runner;
    private readonly HashSet<PlayerRef> _spawningPlayers = new();

    private void Awake()
    {
        runner = GetComponent<NetworkRunner>();
        if (runner != null)
            runner.AddCallbacks(this);
    }

    private void OnDestroy()
    {
        if (runner != null)
            runner.RemoveCallbacks(this);
    }

    private Transform[] FindSpawnPoints()
    {
        var found = GameObject.FindGameObjectsWithTag(spawnPointTag);
        var points = new Transform[found.Length];
        for (int i = 0; i < found.Length; i++)
            points[i] = found[i].transform;
        return points;
    }

    private static bool IsLobbyScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName == "Scene-Server";
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[PlayerSpawner] Player joined: {player.PlayerId}, Local: {runner.LocalPlayer.PlayerId}");

        if (!runner.IsServer)
            return;

        if (IsLobbyScene())
        {
            Debug.Log($"[PlayerSpawner] In lobby scene, skipping spawn for player {player.PlayerId}");
            return;
        }

        foreach (var obj in runner.GetAllNetworkObjects())
        {
            if (obj != null && obj.InputAuthority == player)
            {
                Debug.Log($"[PlayerSpawner] Player {player.PlayerId} already spawned, skipping.");
                return;
            }
        }

        TrySpawnPlayer(player);
    }

    private async void TrySpawnPlayer(PlayerRef player)
    {
        try
        {
            if (runner == null || !runner.IsRunning)
                return;

            if (!_spawningPlayers.Add(player))
            {
                Debug.Log($"[PlayerSpawner] Player {player.PlayerId} spawn already in progress, skipping.");
                return;
            }

            var points = FindSpawnPoints();
            NetworkObject prefab = ResolvePrefabForPlayer(player);
            if (prefab == null)
            {
                Debug.LogError($"PlayerSpawner: No valid player prefab for player {player.PlayerId}.");
                _spawningPlayers.Remove(player);
                return;
            }

            Vector3 spawnPos = Vector3.zero;
            if (points.Length > 0)
            {
                int pointIndex = player.PlayerId % points.Length;
                spawnPos = points[pointIndex].position;

                if (points.Length == 1)
                {
                    float angle = (player.PlayerId * 45f) * Mathf.Deg2Rad;
                    spawnPos += new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 0.5f;
                }
            }

            await runner.SpawnAsync(prefab, spawnPos, Quaternion.identity, player);
            Debug.Log($"Spawned {prefab.name} for player {player.PlayerId} at {spawnPos}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerSpawner] Spawn failed: {e.Message}");
        }
        finally
        {
            _spawningPlayers.Remove(player);
        }
    }

    private NetworkObject ResolvePrefabForPlayer(PlayerRef player)
    {
        if (playerPrefabs == null || playerPrefabs.Length == 0)
            return null;

        if (player != runner.LocalPlayer)
        {
            int prefabIndex = player.PlayerId % playerPrefabs.Length;
            return playerPrefabs[prefabIndex];
        }

        NetworkObject selectedPrefab = ResolveSelectedCharacterPrefab();
        if (selectedPrefab != null)
            return selectedPrefab;

        Debug.LogWarning("PlayerSpawner: Selected character prefab is missing. Falling back to indexed prefab.");

        int fallbackIndex = Mathf.Clamp(GameSessionData.SelectedCharacterIndex, 0, playerPrefabs.Length - 1);
        return playerPrefabs[fallbackIndex];
    }

    private NetworkObject ResolveSelectedCharacterPrefab()
    {
        GameObject selectedPrefab = GameSessionData.SelectedCharacterPrefab;
        if (selectedPrefab == null)
            return null;

        if (selectedPrefab.TryGetComponent(out NetworkObject networkObject))
            return networkObject;

        Debug.LogError($"PlayerSpawner: Selected prefab '{selectedPrefab.name}' has no NetworkObject component.");
        return null;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        foreach (var obj in runner.GetAllNetworkObjects())
        {
            if (obj != null && obj.InputAuthority == player)
            {
                runner.Despawn(obj);
                break;
            }
        }
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        if (IsLobbyScene())
        {
            Debug.Log($"[PlayerSpawner] In lobby scene, skipping scene-load spawn.");
            return;
        }

        int playerCount = 0;
        foreach (var _ in runner.ActivePlayers) playerCount++;
        Debug.Log($"[PlayerSpawner] Scene load done. Spawning {playerCount} players...");
        foreach (var player in runner.ActivePlayers)
        {
            bool alreadySpawned = false;
            foreach (var obj in runner.GetAllNetworkObjects())
            {
                if (obj != null && obj.InputAuthority == player)
                {
                    alreadySpawned = true;
                    break;
                }
            }

            if (!alreadySpawned)
                TrySpawnPlayer(player);
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[PlayerSpawner] Runner shutdown: {shutdownReason}");
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
