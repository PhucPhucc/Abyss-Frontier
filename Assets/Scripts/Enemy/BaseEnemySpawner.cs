using System.Collections.Generic;
using Fusion;
using UnityEngine;

public abstract class BaseEnemySpawner : TilemapSpawnBase
{
    [Header("Base Settings")]
    public GameObject[] enemyPrefabs; // Danh sách quái có thể spawn
    public int maxEnemies = 5;
    
    [Header("Enemy Level Settings")]
    public EnemyLevel spawnLevel = EnemyLevel.Level1;
    
    // Lưu các quái đang hoạt động
    protected List<GameObject> activeEnemies = new List<GameObject>();

    protected int currentEnemyCount
    {
        get
        {
            CleanActiveEnemiesList();
            return activeEnemies.Count;
        }
    }

    // Các lớp con bắt buộc phải tự triển khai logic spawn
    public abstract void SpawnEnemies();

    // Hàm tiện ích dùng chung để tạo quái
    protected GameObject InstantiateEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject enemy;

        if (GameSessionData.IsMultiplayer)
        {
            NetworkRunner runner = GameLauncher.CurrentRunner;
            if (runner == null || !runner.IsServer)
                return null;

            NetworkSpawnOp spawnOp = runner.SpawnAsync(prefab, position, rotation);
            enemy = spawnOp.Object != null ? spawnOp.Object.gameObject : null;
            if (enemy == null) return null;
        }
        else
        {
            enemy = Instantiate(prefab, position, rotation);
        }

        activeEnemies.Add(enemy);

        if (enemy.TryGetComponent(out EnemyHealth health))
        {
            health.Died += () =>
            {
                activeEnemies.Remove(enemy);
                HandleEnemyDeath();
            };

            InitializeEnemyStatsViaReflection(health, spawnLevel);
        }

        return enemy;
    }

    private void InitializeEnemyStatsViaReflection(EnemyHealth health, EnemyLevel level)
    {
        try
        {
            var bindingFlags = System.Reflection.BindingFlags.NonPublic | 
                               System.Reflection.BindingFlags.Public | 
                               System.Reflection.BindingFlags.Instance;

            // 1. Gán trường 'enemyLevel'
            var levelField = typeof(EnemyHealth).GetField("enemyLevel", bindingFlags);
            if (levelField != null)
            {
                levelField.SetValue(health, level);
            }

            // 2. Lấy trường 'statsDefinition'
            var statsField = typeof(EnemyHealth).GetField("statsDefinition", bindingFlags);
            if (statsField != null)
            {
                var statsDef = statsField.GetValue(health) as EnemyStats;
                if (statsDef != null)
                {
                    int lvl = (int)level;
                    int maxHP = statsDef.GetHP(lvl);
                    int defValue = statsDef.GetDEF(lvl);

                    // 3. Gán các trường private: 'maxHealth', 'def', 'currentHealth'
                    typeof(EnemyHealth).GetField("maxHealth", bindingFlags)?.SetValue(health, maxHP);
                    typeof(EnemyHealth).GetField("def", bindingFlags)?.SetValue(health, defValue);
                    typeof(EnemyHealth).GetField("currentHealth", bindingFlags)?.SetValue(health, maxHP);

                    // 4. Lấy component 'enemyAI' và gọi 'SetStatsFromDefinition' trên nó
                    var aiField = typeof(EnemyHealth).GetField("enemyAI", bindingFlags);
                    if (aiField != null)
                    {
                        var ai = aiField.GetValue(health) as EnemyAI;
                        if (ai != null)
                        {
                            ai.SetStatsFromDefinition(statsDef, lvl);
                        }
                    }

                    // 5. Gọi method 'NotifyHealthChanged' để cập nhật giao diện
                    var notifyMethod = typeof(EnemyHealth).GetMethod("NotifyHealthChanged", bindingFlags);
                    notifyMethod?.Invoke(health, null);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BaseEnemySpawner] Lỗi Reflection khi khởi tạo Enemy: {ex.Message}");
        }
    }

    protected virtual void HandleEnemyDeath()
    {
        // Có thể mở rộng ở các lớp con (ví dụ: PatrolRespawnSpawner hồi sinh)
    }

    protected void CleanActiveEnemiesList()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    // Lọc các vị trí hợp lệ của Tilemap nằm trong một Collider2D cụ thể
    protected List<Vector3> GetValidPositionsInCollider(Collider2D collider)
    {
        List<Vector3> result = new List<Vector3>();
        if (collider == null) return result;

        foreach (Vector3 pos in validSpawnPositions)
        {
            if (collider.OverlapPoint(pos))
            {
                result.Add(pos);
            }
        }
        return result;
    }

    // Cưỡng ép tọa độ thế giới về tâm ô Tilemap và kiểm tra tính hợp lệ
    protected Vector3 GetSafeSpawnPosition(Vector3 position)
    {
        if (backgroundMap == null) return position;

        // Chuyển đổi vị trí sang ô Grid và lấy tâm thế giới của ô đó
        Vector3Int cellPos = backgroundMap.WorldToCell(position);
        Vector3 cellCenter = backgroundMap.GetCellCenterWorld(cellPos);
        cellCenter.z = 0f; // Đảm bảo Z = 0 cho game 2D

        // Kiểm tra xem vị trí ô này có nằm trong danh sách các vị trí hợp lệ hay không
        foreach (Vector3 validPos in validSpawnPositions)
        {
            if (Vector3.Distance(validPos, cellCenter) < 0.1f)
            {
                return validPos;
            }
        }

        // Nếu không hợp lệ (ngoài map hoặc có vật cản), tìm vị trí hợp lệ gần nhất
        if (validSpawnPositions.Count > 0)
        {
            Vector3 nearestPos = validSpawnPositions[0];
            float minDistance = Vector3.Distance(position, nearestPos);
            
            for (int i = 1; i < validSpawnPositions.Count; i++)
            {
                float distance = Vector3.Distance(position, validSpawnPositions[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPos = validSpawnPositions[i];
                }
            }
            return nearestPos;
        }

        return cellCenter;
    }
}