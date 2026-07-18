using System.Collections;
using UnityEngine;

public class PatrolRespawnSpawner : BaseEnemySpawner
{
    //Tuần tra, chết thì spawn lại
    //Lớp này sẽ cần thêm logic thời gian chờ (cooldown) để hồi sinh và truyền danh sách các điểm tuần tra (Waypoints) cho quái vật.
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
            
            // Gán waypoints cho enemyAI thông qua Reflection để tránh sửa file EnemyAI.cs
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
        SpawnEnemies();
    }
}