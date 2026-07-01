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
    private Transform[] spawnPoints;
    private bool alreadySpawning;

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

    public static int ResolvePrefabIndex(int selectedCharacterIndex, int prefabCount)
    {
        if (prefabCount <= 0)
            return 0;

        return Mathf.Clamp(selectedCharacterIndex, 0, prefabCount - 1);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[PlayerSpawner] Player joined: {player.PlayerId}, Local: {runner.LocalPlayer.PlayerId}");

        if (!runner.IsServer)
            return;

        TrySpawnPlayer(player);
    }

    private async void TrySpawnPlayer(PlayerRef player)
    {
        if (alreadySpawning) return;
        alreadySpawning = true;

        if (runner == null || !runner.IsRunning)
        {
            alreadySpawning = false;
            return;
        }

        if (playerPrefabs == null || playerPrefabs.Length == 0 || playerPrefabs[0] == null)
        {
            Debug.LogError("PlayerSpawner: No valid player prefab assigned!");
            alreadySpawning = false;
            return;
        }

        var points = FindSpawnPoints();

        int prefabIndex;
        if (player == runner.LocalPlayer)
            prefabIndex = ResolvePrefabIndex(GameSessionData.SelectedCharacterIndex, playerPrefabs.Length);
        else
            prefabIndex = ResolvePrefabIndex(player.PlayerId, playerPrefabs.Length);

        NetworkObject prefab = playerPrefabs[prefabIndex];
        Vector3 spawnPos = points.Length > 0 ? points[0].position : Vector3.zero;

        await runner.SpawnAsync(prefab, spawnPos, Quaternion.identity, player);
        Debug.Log($"Spawned {prefab.name} for player {player.PlayerId} at {spawnPos}");

        alreadySpawning = false;
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
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[PlayerSpawner] Runner shutdown: {shutdownReason}");
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
