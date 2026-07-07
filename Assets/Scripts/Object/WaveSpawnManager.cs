using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro; // Nếu dùng TextMeshPro cho UI

public class WaveSpawnManager : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap backgroundMap;
    [SerializeField] private Tilemap[] obstacleMaps; // Kéo wall-base, wall-top, wall-bottom vào đây

    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] itemPrefabs; // Danh sách các loại vật phẩm
    [SerializeField] private int itemsPerWave = 20;
    [SerializeField] private int totalWaves = 3;
    [SerializeField] private float timePerWave = 30f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI waveText;

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> activeItems = new List<GameObject>();
    private int currentWave = 0;

    void Start()
    {
        FindValidSpawnPositions();
        StartCoroutine(GameLoopCoroutine());
    }

    // Quét bản đồ để tìm các tọa độ không có tường
    private void FindValidSpawnPositions()
    {
        BoundsInt bounds = backgroundMap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // Nếu có tile ở background
                if (backgroundMap.HasTile(cellPosition))
                {
                    bool hasObstacle = false;

                    // Kiểm tra xem có tile ở bất kỳ layer tường nào không
                    foreach (var obstacleMap in obstacleMaps)
                    {
                        if (obstacleMap.HasTile(cellPosition))
                        {
                            hasObstacle = true;
                            break;
                        }
                    }

                    // Nếu không có tường, thêm tọa độ thế giới (world point) vào danh sách an toàn
                    if (!hasObstacle)
                    {
                        // Lấy tâm của ô tile để vật phẩm nằm ngay giữa
                        validSpawnPositions.Add(backgroundMap.GetCellCenterWorld(cellPosition));
                    }
                }
            }
        }
    }

    private IEnumerator GameLoopCoroutine()
    {
        while (currentWave < totalWaves)
        {
            currentWave++;
            if (waveText != null) waveText.text = "Wave: " + currentWave + "/" + totalWaves;

            SpawnItems();

            // Đếm ngược thời gian
            float timer = timePerWave;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                UpdateTimerUI(timer);
                yield return null; // Đợi frame tiếp theo
            }

            // Hết thời gian, dọn dẹp vật phẩm còn sót lại để chuẩn bị đợt mới
            ClearRemainingItems();

            // Có thể thêm yield return new WaitForSeconds(2f); ở đây nếu muốn thời gian nghỉ giữa các đợt
        }

        Debug.Log("Game Over! Đã hoàn thành 3 đợt.");
        // Gọi hàm kết thúc game, hiển thị bảng điểm...
    }

    private void UpdateTimerUI(float time)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(time);
            timerText.text = "Time: " + seconds.ToString();
        }
    }

    private void SpawnItems()
    {
        if (validSpawnPositions.Count == 0 || itemPrefabs.Length == 0) return;

        // Tạo một bản sao của danh sách vị trí để không spawn 2 vật phẩm trùng một ô trong cùng 1 wave
        List<Vector3> availablePositions = new List<Vector3>(validSpawnPositions);

        int spawnCount = Mathf.Min(itemsPerWave, availablePositions.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            // Chọn ngẫu nhiên 1 vị trí
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector3 spawnPos = availablePositions[randomIndex];

            // Chọn ngẫu nhiên 1 loại vật phẩm
            GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

            // Spawn và lưu trữ
            GameObject spawnedItem = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
            activeItems.Add(spawnedItem);

            // Xóa vị trí vừa spawn khỏi danh sách tạm để tránh trùng lặp
            availablePositions.RemoveAt(randomIndex);
        }
    }

    private void ClearRemainingItems()
    {
        foreach (var item in activeItems)
        {
            if (item != null) // Kiểm tra nếu vật phẩm chưa bị player ăn
            {
                Destroy(item);
            }
        }
        activeItems.Clear();
    }
}