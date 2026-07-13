using System.Collections;
using UnityEngine;

public class PatrolRespawnSpawner : BaseEnemySpawner
{
    //Tuần tra, chết thì spawn lại
    //Lớp này sẽ cần thêm logic thời gian chờ (cooldown) để hồi sinh và truyền danh sách các điểm tuần tra (Waypoints) cho quái vật.
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public Transform spawnPoint;
    public float respawnDelay = 3f;
    
    private void Start()
    {
        SpawnEnemies();
    }

    public override void SpawnEnemies()
    {
        if (currentEnemyCount < maxEnemies)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = InstantiateEnemy(prefab, spawnPoint.position, spawnPoint.rotation);
            
            // Gán waypoints cho enemy (giả sử có script PatrolController)
            // enemy.GetComponent<PatrolController>().SetWaypoints(waypoints);
        }
    }

    protected override void HandleEnemyDeath()
    {
        base.HandleEnemyDeath();
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnEnemies();
    }
}