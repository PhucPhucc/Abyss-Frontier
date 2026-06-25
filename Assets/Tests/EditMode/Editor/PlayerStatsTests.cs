using NUnit.Framework;
using UnityEngine;

public class PlayerStatsTests
{
    [Test]
    public void AddExperienceAwardsStatPointsAndKeepsRemainder()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();

        // stats starts with level=1, availableStatPoints=5, currentExp=0, expToNextLevel=100.
        // derivedExpMultiplier = 1.1 (since intelligence=1).
        // If we add 100 raw experience:
        // finalExp = 100 * 1.1 = 110.
        // It levels up once: currentExp becomes 110 - 100 = 10.
        // statPoints becomes 5 + 5 = 10.
        stats.AddExperience(100);

        Assert.AreEqual(10, stats.StatPoints);
        Assert.AreEqual(10, stats.CurrentExperience);
        Assert.AreEqual(2, stats.Level);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void UpgradeStrengthConsumesPointAndIncreasesAttackDamage()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();

        int initialDamage = stats.AttackDamage; // Starts at 5 + 1 * 2 = 7
        int initialPoints = stats.StatPoints;   // Starts at 5

        bool upgraded = stats.AllocateStat(StatType.Strength);

        Assert.IsTrue(upgraded);
        Assert.AreEqual(initialPoints - 1, stats.StatPoints);
        Assert.AreEqual(initialDamage + 2, stats.AttackDamage); // Strength + 1 increases ATK by 2

        Object.DestroyImmediate(player);
    }

    [Test]
    public void UpgradeWithoutStatPointsDoesNotChangeStats()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();

        // Consume all 5 stat points
        for (int i = 0; i < 5; i++)
        {
            stats.AllocateStat(StatType.Strength);
        }

        Assert.AreEqual(0, stats.StatPoints);
        int finalDamage = stats.AttackDamage;

        bool upgraded = stats.AllocateStat(StatType.Strength);

        Assert.IsFalse(upgraded);
        Assert.AreEqual(0, stats.StatPoints);
        Assert.AreEqual(finalDamage, stats.AttackDamage);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void RestoreVitalsRefillsHealthAndStamina()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();
        PlayerHealth health = player.AddComponent<PlayerHealth>();

        // Spend some stamina
        stats.SpendStamina(40f);
        Assert.AreEqual(60f, stats.CurrentStamina);

        // Take damage
        stats.TakeDamage(20);
        Assert.AreEqual(stats.MaxHealth - 20, stats.CurrentHealth);

        // Restore
        stats.RestoreVitals();

        Assert.AreEqual(stats.MaxHealth, stats.CurrentHealth);
        Assert.AreEqual(stats.MaxStamina, stats.CurrentStamina);

        Object.DestroyImmediate(player);
    }
}
