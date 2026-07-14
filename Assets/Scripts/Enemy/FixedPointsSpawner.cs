using UnityEngine;

public class FixedPointsSpawner : BaseEnemySpawner
{
    [Header("Fixed Points Settings")]
    public Transform[] spawnPoints;

    private void Start()
    {
        // Để trống để quái không tự spawn lúc đầu game
    }

    public override void SpawnEnemies()
    {
        Debug.Log($"[Spawner] Bắt đầu quét danh sách điểm spawn. Số lượng quái hiện tại: {currentEnemyCount}/{maxEnemies}");

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[Spawner] Danh sách 'spawnPoints' đang trống! Hãy kéo thả các điểm spawn vào Inspector.");
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("[Spawner] Danh sách 'enemyPrefabs' đang trống! Hãy kéo thả Prefab quái vào Inspector.");
            return;
        }

        int successSpawnCount = 0;

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
            {
                Debug.LogWarning("[Spawner] Phát hiện một phần tử trong danh sách spawnPoints bị Null! Bỏ qua.");
                continue;
            }

            if (currentEnemyCount >= maxEnemies)
            {
                Debug.LogWarning($"[Spawner] Đã đạt giới hạn tối đa quái vật ({maxEnemies}). Dừng quá trình spawn tại điểm: {point.name}");
                break;
            }

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector3 safePos = GetSafeSpawnPosition(point.position);

            InstantiateEnemy(prefab, safePos, point.rotation);
            successSpawnCount++;

            Debug.Log($"<color=cyan>[Spawner SUCCESS]</color> Đã spawn quái [{prefab.name}] tại điểm: {point.name} (Vị trí thực tế: {safePos})");
        }

        Debug.Log($"[Spawner] Kết thúc quá trình gọi lệnh. Đã thêm mới {successSpawnCount} quái vật vào map trong lượt này.");
    }
}