using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class WaveSpawnManager : MonoBehaviour
{
    // Tạo cấu trúc dữ liệu tùy chỉnh cho từng Wave trong Inspector
    [System.Serializable]
    public struct WaveSettings
    {
        public GameObject itemPrefab; // Loại vật phẩm xuất hiện trong wave này
        public int itemsPerWave; // Số lượng vật phẩm cần spawn cho wave này
    }

    [Header("Tilemap References")]
    [SerializeField] private Tilemap backgroundMap;
    [SerializeField] private Tilemap[] obstacleMaps;

    [Header("Spawn Area Settings")]
    [Tooltip("Gán BoxCollider2D hoặc PolygonCollider2D (Is Trigger) để giới hạn vùng spawn item. Để trống nếu muốn spawn toàn bộ map.")]
    [SerializeField] private Collider2D spawnArea;

    [Header("Wave Configuration")]
    [SerializeField] private List<WaveSettings> waves = new List<WaveSettings>();

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> activeItems = new List<GameObject>();

    private int currentWaveIndex = 0; // Chỉ số mảng (bắt đầu từ 0)
    private bool isWaveActive = false; 

    void Start()
    {
        if (waves.Count == 0)
        {
            Debug.LogError("Chưa cấu hình danh sách Waves trong Inspector!");
            return;
        }

        // Bước 1: Quét các vị trí hợp lệ dựa vào Tilemap và Spawn Area
        FindValidSpawnPositions();

        // Bước 2: Bắt đầu wave đầu tiên
        StartWave(currentWaveIndex);
    }

    void Update()
    {
        if (isWaveActive)
        {
            CheckWaveProgress();
        }
    }

    private void CheckWaveProgress()
    {
        // Loại bỏ các item đã bị Destroy (khi player ăn trúng item)
        activeItems.RemoveAll(item => item == null);

        // Nếu ăn sạch toàn bộ item hiện tại
        if (activeItems.Count == 0)
        {
            isWaveActive = false;
            Debug.Log("Đã thu thập hết sạch item của wave này!");
            
            // Chuyển sang chỉ số wave tiếp theo
            currentWaveIndex++;
            StartWave(currentWaveIndex);
        }
    }

    public void StartWave(int waveIndex)
    {
        // Kiểm tra xem đã hoàn thành tất cả các wave được cấu hình chưa
        if (waveIndex >= waves.Count)
        {
            Debug.Log("Xin chúc mừng! Bạn đã hoàn thành tất cả các đợt wave.");
            return;
        }

        WaveSettings currentWaveConfig = waves[waveIndex];
        
        Debug.Log("Start Wave " + (waveIndex + 1) + ": Spawning " + currentWaveConfig.itemsPerWave + " items of type [" + currentWaveConfig.itemPrefab.name + "]");

        // Thực hiện spawn item cho wave dựa trên cấu hình struct
        SpawnItemsForWave(currentWaveConfig);

        isWaveActive = true;
    }

    private void SpawnItemsForWave(WaveSettings config)
    {
        if (validSpawnPositions.Count == 0)
        {
            Debug.LogError("Không có vị trí hợp lệ nào được tìm thấy trên bản đồ để spawn!");
            return;
        }

        if (config.itemPrefab == null)
        {
            Debug.LogError("Prefab của wave {currentWaveIndex + 1} đang bị trống (Null)!");
            return;
        }

        List<Vector3> availablePositions = new List<Vector3>(validSpawnPositions);
        int spawnCount = Mathf.Min(config.itemsPerWave, availablePositions.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector3 spawnPos = availablePositions[randomIndex];

            GameObject spawnedItem = Instantiate(config.itemPrefab, spawnPos, Quaternion.identity);
            activeItems.Add(spawnedItem);

            availablePositions.RemoveAt(randomIndex);
        }

        Debug.Log("Đã tạo ra thành công " + spawnCount + "/" + config.itemsPerWave + " vật phẩm [" + config.itemPrefab.name + "]");
    }

    private void FindValidSpawnPositions()
    {
        BoundsInt bounds = backgroundMap.cellBounds;
        validSpawnPositions.Clear();
        int scannedCount = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                if (backgroundMap.HasTile(cellPosition))
                {
                    Vector3 worldPos = backgroundMap.GetCellCenterWorld(cellPosition);

                    // THÊM: Kiểm tra xem vị trí ô này có nằm BÊN TRONG Spawn Area Collider hay không (nếu có gán area)
                    if (spawnArea != null && !spawnArea.OverlapPoint(worldPos))
                    {
                        continue; // Bỏ qua nếu ô này nằm ngoài vùng Collider chỉ định
                    }

                    // Kiểm tra xem vị trí này có chứa chướng ngại vật (vật cản) không
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
                        validSpawnPositions.Add(worldPos);
                        scannedCount++;
                    }
                }
            }
        }

        Debug.Log("Khởi tạo vùng spawn hoàn tất! Tìm thấy " + validSpawnPositions.Count + " ô Tilemap hợp lệ thích hợp để đặt vật phẩm.");
    }
}