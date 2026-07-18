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

            EnemyRespawnRunner.RespawnAllAtHub();

            Assert.IsTrue(enemy.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            EnemyRespawnRunner.RespawnAllAtHub();
        }
    }

    [Test]
    public void RespawnAllAtHubRestoresColliderAndLivingFlags()
    {
        GameObject enemy = new GameObject("CorpseClone");
        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        EnemyHealth health = enemy.AddComponent<EnemyHealth>();
        EnemyAI ai = enemy.AddComponent<EnemyAI>();

        try
        {
            // Simulate buggy clone copied after Die (disabled collider + dead AI/health).
            collider.enabled = false;
            ai.OnDeath();
            typeof(EnemyHealth)
                .GetField("isDead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(health, true);

            enemy.SetActive(false);
            EnemyRespawnRunner.RegisterForHubRespawn(enemy);
            EnemyRespawnRunner.RespawnAllAtHub();

            Assert.IsTrue(enemy.activeSelf);
            Assert.IsTrue(collider.enabled, "Hub respawn must re-enable collider so player attacks can hit.");
            Assert.IsFalse(health.IsDead);
            Assert.IsFalse(ai.IsDead);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            EnemyRespawnRunner.RespawnAllAtHub();
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
