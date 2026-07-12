using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

// Tạo một lớp cấu hình để quản lý từng đợt linh hoạt hơn
[System.Serializable]
public class WaveConfig
{
    [Tooltip("Kéo thả 1 hoặc nhiều loại Enemy vào đây. Code sẽ random loại để spawn.")]
    public GameObject[] enemyPrefabs;

    [Tooltip("Số lượng quái vật tối đa sẽ xuất hiện trong đợt này.")]
    public int enemiesPerWave = 10;
}

public class EnemySpawn : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap backgroundMap;
    [SerializeField] private Tilemap[] obstacleMaps;

    [Header("Wave Settings")]
    [Tooltip("Tùy chỉnh số đợt và loại quái cho từng đợt")]
    [SerializeField] private WaveConfig[] waves;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveText;

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> activeEnemies = new List<GameObject>();

    private int currentWaveIndex = 0;
    private bool isWaveActive = false;

    void Start()
    {
        // 1. Quét map để tìm các vị trí có ground mà không bị đè bởi wall
        FindValidSpawnPositions();

        // 2. Bắt đầu ngay đợt 1
        NextWave();
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
        // Khi quái chết (bị Destroy), phần tử đó trong list sẽ biến thành null.
        // Lệnh này dọn dẹp các quái đã chết khỏi danh sách quản lý.
        activeEnemies.RemoveAll(enemy => enemy == null);

        // Nếu list rỗng nghĩa là toàn bộ quái đợt này đã bị tiêu diệt
        if (activeEnemies.Count == 0)
        {
            isWaveActive = false;
            Debug.Log("<color=green>[SpawnManager]</color> Đã tiêu diệt hết quái! Chuẩn bị sang wave tiếp theo...");
            NextWave();
        }
    }

    public void NextWave()
    {
        // Kiểm tra xem đã hết số đợt thiết lập chưa
        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log("<color=yellow>[SpawnManager]</color> Xin chúc mừng! Bạn đã hoàn thành tất cả các đợt.");
            if (waveText != null) waveText.text = "Completed!";
            return;
        }

        // Lấy cấu hình của đợt hiện tại
        WaveConfig currentWaveConfig = waves[currentWaveIndex];
        currentWaveIndex++; // Tăng biến đếm cho UI và đợt sau

        Debug.Log($"<color=cyan>[SpawnManager]</color> ====== BẮT ĐẦU WAVE {currentWaveIndex}/{waves.Length} ======");

        if (waveText != null)
            waveText.text = "Wave: " + currentWaveIndex + "/" + waves.Length;

        // Gọi hàm spawn
        SpawnEnemies(currentWaveConfig);

        isWaveActive = true;
    }

    private void SpawnEnemies(WaveConfig waveConfig)
    {
        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("Không tìm thấy vị trí mặt đất hợp lệ nào để spawn!");
            return;
        }

        if (waveConfig.enemyPrefabs == null || waveConfig.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"Wave {currentWaveIndex} chưa được gán Prefab quái vật nào!");
            return;
        }

        // Tạo một list tạm để khi spawn xong ở 1 ô, ta xóa ô đó đi (tránh quái đè lên nhau)
        List<Vector3> availablePositions = new List<Vector3>(validSpawnPositions);

        // Spawn theo số lượng yêu cầu hoặc giới hạn ở số lượng ô đất trống
        int spawnCount = Mathf.Min(waveConfig.enemiesPerWave, availablePositions.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            // 1. Chọn ngẫu nhiên vị trí
            int randomPosIndex = Random.Range(0, availablePositions.Count);
            Vector3 spawnPos = availablePositions[randomPosIndex];

            // 2. Chọn ngẫu nhiên 1 loại quái vật trong danh sách được phép của Wave này
            int randomEnemyIndex = Random.Range(0, waveConfig.enemyPrefabs.Length);
            GameObject selectedEnemyPrefab = waveConfig.enemyPrefabs[randomEnemyIndex];

            // 3. Spawn
            GameObject spawnedEnemy = Instantiate(selectedEnemyPrefab, spawnPos, Quaternion.identity);
            activeEnemies.Add(spawnedEnemy);

            // 4. Bỏ vị trí này khỏi danh sách tạm để vòng lặp sau không chọn lại
            availablePositions.RemoveAt(randomPosIndex);
        }

        Debug.Log($"<color=yellow>[SpawnManager]</color> Đã spawn {spawnCount} quái vật.");
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

                // Nếu có Tile nền đất
                if (backgroundMap.HasTile(cellPosition))
                {
                    bool hasWall = false;

                    // Kiểm tra tất cả các Tilemap tường/vật cản
                    foreach (var wallMap in obstacleMaps)
                    {
                        if (wallMap.HasTile(cellPosition))
                        {
                            hasWall = true;
                            break;
                        }
                    }

                    // Nếu không có tường đè lên, vị trí này là hợp lệ
                    if (!hasWall)
                    {
                        validSpawnPositions.Add(backgroundMap.GetCellCenterWorld(cellPosition));
                    }
                }
            }
        }
    }
}