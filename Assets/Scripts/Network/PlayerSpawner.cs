using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkObject[] playerPrefabs;
    [SerializeField] private string spawnPointTag = "SpawnPoint";

    private NetworkRunner runner;
    private Transform[] spawnPoints;

    private void Awake()
    {
        runner = GetComponent<NetworkRunner>();
        if (runner != null)
            runner.AddCallbacks(this);

        var found = GameObject.FindGameObjectsWithTag(spawnPointTag);
        spawnPoints = new Transform[found.Length];
        for (int i = 0; i < found.Length; i++)
            spawnPoints[i] = found[i].transform;
    }

    private void OnDestroy()
    {
        if (runner != null)
            runner.RemoveCallbacks(this);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogError("PlayerSpawner: No player prefabs assigned!");
            return;
        }

        NetworkObject prefab = playerPrefabs[player.PlayerId % playerPrefabs.Length];
        Vector3 spawnPos = spawnPoints.Length > 0
            ? spawnPoints[player.PlayerId % spawnPoints.Length].position
            : Vector3.zero;
        NetworkObject spawned = runner.Spawn(prefab, spawnPos, Quaternion.identity, player);

        Debug.Log($"Spawned {prefab.name} for Player {player.PlayerId} at {spawnPos}");
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
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
