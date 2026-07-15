using UnityEngine;

/// <summary>
/// Quản lý việc sinh ra các enemy nhỏ hơn khi enemy hiện tại chết.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class SlimeSplitter : MonoBehaviour
{
    [Header("Split Settings")]
    [Tooltip("Prefab của quái vật con sẽ được sinh ra (VD: Slime2 hoặc Slime1)")]
    [SerializeField] private GameObject prefabToSpawn;
    
    [Tooltip("Số lượng quái vật con sinh ra")]
    [SerializeField] private int spawnCount = 2;
    
    [Tooltip("Độ lệch vị trí để các quái vật con không bị đè lên nhau hoàn toàn")]
    [SerializeField] private float spawnOffsetRadius = 0.5f;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        
        // Đăng ký lắng nghe event Died từ EnemyHealth
        if (enemyHealth != null)
        {
            enemyHealth.Died += SpawnChildren;
        }
    }

    private void SpawnChildren()
    {
        if (prefabToSpawn == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            // Tính toán một vị trí ngẫu nhiên xung quanh vị trí quái vật mẹ đang chết
            Vector2 randomOffset = Random.insideUnitCircle * spawnOffsetRadius;
            Vector3 spawnPosition = transform.position + (Vector3)randomOffset;

            // Sinh ra quái vật con
            GameObject child = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            
            // Đảm bảo quái vật con được kích hoạt (phòng trường hợp prefab đang bị tắt)
            child.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký event để tránh rò rỉ bộ nhớ (memory leak) khi object bị Destroy
        if (enemyHealth != null)
        {
            enemyHealth.Died -= SpawnChildren;
        }
    }
}