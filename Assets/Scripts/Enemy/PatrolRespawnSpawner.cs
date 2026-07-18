using System.Collections;
using UnityEngine;

public class PatrolRespawnSpawner : BaseEnemySpawner
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public Transform spawnPoint;
    public float respawnDelay = 3f;
    
    private void Start()
    {
        SpawnEnemies();
    }

    public override void SpawnEnemies()
    {
        if (currentEnemyCount < maxEnemies)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector3 safePos = GetSafeSpawnPosition(spawnPoint.position);
            GameObject enemy = InstantiateEnemy(prefab, safePos, spawnPoint.rotation);

            if (enemy == null) return;
            
            if (enemy.TryGetComponent(out EnemyAI enemyAI))
            {
                try
                {
                    var bindingFlags = System.Reflection.BindingFlags.NonPublic | 
                                       System.Reflection.BindingFlags.Public | 
                                       System.Reflection.BindingFlags.Instance;
                    var waypointsField = typeof(EnemyAI).GetField("waypoints", bindingFlags);
                    if (waypointsField != null)
                    {
                        waypointsField.SetValue(enemyAI, waypoints);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[PatrolRespawnSpawner] Lỗi Reflection khi gán waypoints: {ex.Message}");
                }
            }
        }
    }

    protected override void HandleEnemyDeath()
    {
        base.HandleEnemyDeath();
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (GameSessionData.IsMultiplayer && !GameSessionData.IsHost)
            yield break;

        SpawnEnemies();
    }
}