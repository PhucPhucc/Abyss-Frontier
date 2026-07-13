using UnityEngine;

public class RandomAreaSpawner : BaseEnemySpawner
{
    [Header("Area Settings")]
    public Collider spawnArea;

    private void Start()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemies();
        }
    }

    public override void SpawnEnemies()
    {
        Vector3 randomPoint = GetRandomPointInBounds(spawnArea.bounds);
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        InstantiateEnemy(prefab, randomPoint, Quaternion.identity);
    }

    private Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y), // Hoặc fix trục Y nếu game 2D/phẳng
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}