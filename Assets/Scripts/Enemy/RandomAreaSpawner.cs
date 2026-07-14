using System.Collections.Generic;
using UnityEngine;

public class RandomAreaSpawner : BaseEnemySpawner
{
    [Header("Area Settings")]
    public Collider2D spawnArea;

    private void Start()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemies();
        }
    }

    public override void SpawnEnemies()
    {
        if (currentEnemyCount >= maxEnemies) return;

        Vector3 spawnPoint;
        List<Vector3> validPositions = GetValidPositionsInCollider(spawnArea);

        if (validPositions != null && validPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validPositions.Count);
            spawnPoint = validPositions[randomIndex];
        }
        else
        {
            Bounds bounds = spawnArea != null ? spawnArea.bounds : new Bounds(transform.position, Vector3.one);
            spawnPoint = GetRandomPointInBounds(bounds);
        }

        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            InstantiateEnemy(prefab, spawnPoint, Quaternion.identity);
        }
    }

    private Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            0f // Trục Z bằng 0 cho game 2D
        );
    }
}