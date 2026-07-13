using UnityEngine;

public class FixedPointsSpawner : BaseEnemySpawner
{
    [Header("Fixed Points Settings")]
    public Transform[] spawnPoints;

    private void Start()
    {
        SpawnEnemies();
    }

    public override void SpawnEnemies()
    {
        foreach (Transform point in spawnPoints)
        {
            if (currentEnemyCount >= maxEnemies) break;

            // Tỉ lệ spawn ngẫu nhiên tại mỗi điểm (ví dụ 50%)
            if (Random.value > 0.5f) 
            {
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                InstantiateEnemy(prefab, point.position, point.rotation);
            }
        }
    }
}