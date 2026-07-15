using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiPatrolSpawner : BaseEnemySpawner
{
    [System.Serializable]
    public class PatrolConfig
    {
        [HideInInspector] public string routeName = "Tuyến Tuần Tra 1";
        
        //Bỏ qua phần cấu hình Base Settings

        [Header("Enemy Setting")]
        [Tooltip("Kéo Prefab quái vật cụ thể vào đây (VD: Orc, Slime).")]
        public GameObject specificEnemyPrefab; 
        
        [Header("Patrol Settings")]
        [Tooltip("Danh sách các điểm tuần tra. Quái vật sẽ spawn tại Waypoint đầu tiên (Element 0).")]
        public Transform[] waypoints;
        [HideInInspector] public float respawnDelay = 3f;
        
        [HideInInspector] public GameObject activeEnemy; 
        [HideInInspector] public bool isWaitingRespawn = false;
    }

    [Header("Multi Patrol Routes")]
    public List<PatrolConfig> patrolRoutes = new List<PatrolConfig>();

    private void Start()
    {
        // Tự động gán Max Enemies của lớp cha bằng tổng số lượng tuyến đường bạn tạo ra
        maxEnemies = patrolRoutes.Count; 

        foreach (var route in patrolRoutes)
        {
            SpawnForRoute(route);
        }
    }

    private void Update()
    {
        foreach (var route in patrolRoutes)
        {
            if (route.activeEnemy == null && !route.isWaitingRespawn)
            {
                StartCoroutine(RespawnRoutine(route));
            }
        }
    }

    // ĐÂY LÀ HÀM ĐƯỢC THÊM VÀO ĐỂ FIX LỖI CS0534
    public override void SpawnEnemies()
    {
        // Chúng ta để trống hàm này vì việc spawn giờ đã được xử lý riêng cho từng Route qua hàm SpawnForRoute ở dưới.
        // Hàm này chỉ tồn tại để thỏa mãn yêu cầu của lớp cha BaseEnemySpawner.
    }

    private void SpawnForRoute(PatrolConfig route)
    {
        if (route.specificEnemyPrefab == null)
        {
            Debug.LogError($"<color=red>[MultiPatrolSpawner]</color> Tuyến '{route.routeName}' chưa được gán Specific Enemy Prefab! Vui lòng kiểm tra lại Inspector.");
            return;
        }

        // MỚI: Kiểm tra xem mảng waypoints có hợp lệ và có ít nhất 1 phần tử hay không
        if (route.waypoints == null || route.waypoints.Length == 0)
        {
            Debug.LogError($"<color=red>[MultiPatrolSpawner]</color> Tuyến '{route.routeName}' chưa có Waypoints! Cần ít nhất 1 Waypoint để làm điểm spawn.");
            return;
        }

        // Lấy Waypoint đầu tiên (Element 0) làm điểm sinh ra
        Transform startingWaypoint = route.waypoints[0];
        Vector3 safePos = GetSafeSpawnPosition(startingWaypoint.position);
        
        // Spawn quái tại tọa độ và góc xoay của Waypoint đầu tiên
        GameObject enemy = InstantiateEnemy(route.specificEnemyPrefab, safePos, startingWaypoint.rotation);
        route.activeEnemy = enemy;

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
                    waypointsField.SetValue(enemyAI, route.waypoints);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MultiPatrolSpawner] Lỗi Reflection trên tuyến '{route.routeName}': {ex.Message}");
            }
        }
    }

    private IEnumerator RespawnRoutine(PatrolConfig route)
    {
        route.isWaitingRespawn = true; 
        
        yield return new WaitForSeconds(route.respawnDelay);
        
        SpawnForRoute(route); 
        
        route.isWaitingRespawn = false; 
    }

    protected override void HandleEnemyDeath()
    {
        base.HandleEnemyDeath();
    }
}