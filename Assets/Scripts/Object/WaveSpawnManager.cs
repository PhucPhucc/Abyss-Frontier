using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class WaveSpawnManager : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap backgroundMap;
    [SerializeField] private Tilemap[] obstacleMaps;

    [Header("Spawn Settings")]
    [Tooltip("Kéo đúng 3 loại item vào đây. Đợt 1 sẽ spawn Item 0, Đợt 2 spawn Item 1...")]
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private int itemsPerWave = 20;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveText;

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> activeItems = new List<GameObject>();

    private int currentWave = 0;
    private int totalWaves;
    private bool isWaveActive = false; // Biến cờ để kiểm soát việc đang trong một đợt

    void Start()
    {
        // Tổng số đợt sẽ tự động bằng đúng số lượng loại vật phẩm bạn kéo vào
        totalWaves = itemPrefabs.Length;

        FindValidSpawnPositions();

        // Bắt đầu ngay đợt 1 khi game chạy
        NextWave();
    }

    void Update()
    {
        // Nếu đợt đang diễn ra, liên tục kiểm tra tiến độ thu thập
        if (isWaveActive)
        {
            CheckWaveProgress();
        }
    }

    private void CheckWaveProgress()
    {
        // Khi người chơi chạm vào item và lệnh Destroy(gameObject) được gọi ở script ItemCollect,
        // Item đó trong danh sách activeItems sẽ biến thành 'null'.
        // Lệnh dưới đây sẽ dọn dẹp tất cả các mục 'null' ra khỏi danh sách.
        activeItems.RemoveAll(item => item == null);

        // Nếu danh sách trống trơn, nghĩa là player đã ăn sạch item của đợt này
        if (activeItems.Count == 0)
        {
            isWaveActive = false; // Tạm dừng theo dõi để chuẩn bị chuyển đợt
            Debug.Log("<color=green>[SpawnManager]</color> Đã thu thập hết! Chuẩn bị sang wave tiếp theo...");

            NextWave();
        }
    }

    public void NextWave()
    {
        if (currentWave >= totalWaves)
        {
            Debug.Log("<color=yellow>[SpawnManager]</color> Xin chúc mừng! Bạn đã hoàn thành tất cả các đợt.");
            if (waveText != null) waveText.text = "Completed!";
            return;
        }

        currentWave++;
        Debug.Log($"<color=cyan>[SpawnManager]</color> ====== BẮT ĐẦU WAVE {currentWave}/{totalWaves} ======");

        if (waveText != null)
            waveText.text = "Wave: " + currentWave + "/" + totalWaves;

        SpawnItemsForCurrentWave();

        // Đánh dấu đợt đã bắt đầu để hàm Update bắt đầu kiểm tra
        isWaveActive = true;
    }

    private void SpawnItemsForCurrentWave()
    {
        if (validSpawnPositions.Count == 0) return;

        // Chỉ số của mảng bắt đầu từ 0, nên Wave 1 tương ứng với itemPrefabs[0]
        GameObject currentItemPrefab = itemPrefabs[currentWave - 1];

        List<Vector3> availablePositions = new List<Vector3>(validSpawnPositions);
        int spawnCount = Mathf.Min(itemsPerWave, availablePositions.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector3 spawnPos = availablePositions[randomIndex];

            // Spawn đúng loại vật phẩm của đợt hiện tại
            GameObject spawnedItem = Instantiate(currentItemPrefab, spawnPos, Quaternion.identity);
            activeItems.Add(spawnedItem);

            availablePositions.RemoveAt(randomIndex);
        }

        Debug.Log($"<color=yellow>[SpawnManager]</color> Đã spawn {spawnCount} vật phẩm loại: {currentItemPrefab.name}");
    }

    private void FindValidSpawnPositions()
    {
        BoundsInt bounds = backgroundMap.cellBounds;
        validSpawnPositions.Clear();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                if (backgroundMap.HasTile(cellPosition))
                {
                    bool hasObstacle = false;
                    foreach (var obstacleMap in obstacleMaps)
                    {
                        if (obstacleMap.HasTile(cellPosition))
                        {
                            hasObstacle = true;
                            break;
                        }
                    }

                    if (!hasObstacle)
                    {
                        validSpawnPositions.Add(backgroundMap.GetCellCenterWorld(cellPosition));
                    }
                }
            }
        }
    }
}