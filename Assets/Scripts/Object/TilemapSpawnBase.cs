using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class TilemapSpawnBase : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] protected Tilemap backgroundMap;
    [SerializeField] protected Tilemap[] obstacleMaps;

    // Danh sách lưu trữ tất cả các tọa độ trống trên map (World Position)
    protected List<Vector3> validSpawnPositions = new List<Vector3>();

    protected virtual void Awake()
    {
        FindValidSpawnPositions();
    }

    // Thuật toán quét Tilemap lấy từ script của bạn
    private void FindValidSpawnPositions()
    {
        if (backgroundMap == null)
        {
            Debug.LogError("Chưa gán Background Map!");
            return;
        }

        BoundsInt bounds = backgroundMap.cellBounds;
        validSpawnPositions.Clear();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // Nếu ô đó có gạch nền
                if (backgroundMap.HasTile(cellPosition))
                {
                    bool hasObstacle = false;
                    
                    // Kiểm tra xem ô đó có bị vật cản đè lên không
                    foreach (var obstacleMap in obstacleMaps)
                    {
                        if (obstacleMap != null && obstacleMap.HasTile(cellPosition))
                        {
                            hasObstacle = true;
                            break;
                        }
                    }

                    // Nếu trống trải -> Hợp lệ để đứng
                    if (!hasObstacle)
                    {
                        // Lấy tọa độ tâm của Ô đó trong thế giới thực (World Space)
                        validSpawnPositions.Add(backgroundMap.GetCellCenterWorld(cellPosition));
                    }
                }
            }
        }
        Debug.Log($"[TilemapSpawn] Đã tìm thấy {validSpawnPositions.Count} vị trí spawn hợp lệ.");
    }

}