using NUnit.Framework;
using UnityEngine;

public class HubEnemyRespawnTests
{
    [Test]
    public void QueuedEnemiesReviveOnlyWhenHubRespawnIsTriggered()
    {
        GameObject enemy = new GameObject("QueuedEnemy");

        try
        {
            enemy.SetActive(false);
            EnemyRespawnRunner.RegisterForHubRespawn(enemy);

            Assert.IsFalse(enemy.activeSelf);

            // Simulate waiting in dungeon — hub revive has not been called yet.
            Assert.IsFalse(enemy.activeSelf);

            EnemyRespawnRunner.RespawnAllAtHub();

            Assert.IsTrue(enemy.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            EnemyRespawnRunner.RespawnAllAtHub(); // clear any leftover pending entries
        }
    }

    [Test]
    public void RespawnAllAtHubIgnoresDestroyedEntriesAndClearsQueue()
    {
        GameObject alive = new GameObject("AliveQueued");
        GameObject doomed = new GameObject("DoomedQueued");

        try
        {
            alive.SetActive(false);
            doomed.SetActive(false);

            EnemyRespawnRunner.RegisterForHubRespawn(alive);
            EnemyRespawnRunner.RegisterForHubRespawn(doomed);
            Object.DestroyImmediate(doomed);

            EnemyRespawnRunner.RespawnAllAtHub();

            Assert.IsTrue(alive.activeSelf);

            // Second call should be a no-op (queue cleared).
            alive.SetActive(false);
            EnemyRespawnRunner.RespawnAllAtHub();
            Assert.IsFalse(alive.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(alive);
            EnemyRespawnRunner.RespawnAllAtHub();
        }
    }
}
