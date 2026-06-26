using NUnit.Framework;
using UnityEngine;

public class HealthEventTests
{
    [Test]
    public void PlayerHealthReportsFractionAndRaisesChangeEvents()
    {
        GameObject player = new GameObject("Player");

        try
        {
            PlayerHealth health = player.AddComponent<PlayerHealth>();
            int eventCount = 0;
            int observedCurrent = -1;
            int observedMax = -1;

            health.HealthChanged += (current, max) =>
            {
                eventCount++;
                observedCurrent = current;
                observedMax = max;
            };

            health.TakeDamage(10);

            Assert.AreEqual(60, observedCurrent);
            Assert.AreEqual(70, observedMax);
            Assert.AreEqual(60f / 70f, health.HealthFraction, 0.001f);

            health.RestoreFullHealth();

            Assert.AreEqual(70, health.CurrentHealth);
            Assert.AreEqual(1f, health.HealthFraction, 0.001f);
            Assert.GreaterOrEqual(eventCount, 2);
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void EnemyHealthReportsFractionAndRaisesChangeEvents()
    {
        GameObject enemy = new GameObject("Enemy");

        try
        {
            EnemyHealth health = enemy.AddComponent<EnemyHealth>();
            int eventCount = 0;
            int observedCurrent = -1;
            int observedMax = -1;

            health.HealthChanged += (current, max) =>
            {
                eventCount++;
                observedCurrent = current;
                observedMax = max;
            };

            health.TakeDamage(5, Vector2.right);

            Assert.AreEqual(25, observedCurrent);
            Assert.AreEqual(30, observedMax);
            Assert.AreEqual(25f / 30f, health.HealthFraction, 0.001f);
            Assert.AreEqual(1, eventCount);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
        }
    }
}
