using UnityEngine;

public abstract class BaseEnemySpawner : MonoBehaviour
{
    [Header("Base Settings")]
    public GameObject[] enemyPrefabs; // Danh sách quái có thể spawn
    public int maxEnemies = 5;
    
    protected int currentEnemyCount = 0;

    // Các lớp con bắt buộc phải tự triển khai logic spawn
    public abstract void SpawnEnemies();

    // Hàm tiện ích dùng chung để tạo quái
    protected GameObject InstantiateEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // Có thể tích hợp Object Pooling ở đây thay vì Instantiate thông thường
        GameObject enemy = Instantiate(prefab, position, rotation);
        currentEnemyCount++;
        
        // Giả sử Enemy có một component EnemyController, ta đăng ký sự kiện chết để quản lý số lượng
        /*
        if (enemy.TryGetComponent(out EnemyController controller))
        {
            controller.OnDeath += HandleEnemyDeath;
        }
        */
        return enemy;
    }

    protected virtual void HandleEnemyDeath()
    {
        currentEnemyCount--;
    }
}