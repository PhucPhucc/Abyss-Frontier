using NUnit.Framework;
using UnityEngine;

public class PlayerStatsTests
{
    [Test]
    public void AddExperienceAwardsStatPointsAndKeepsRemainder()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();

        stats.AddExperience(250);

        Assert.AreEqual(2, stats.StatPoints);
        Assert.AreEqual(50, stats.CurrentExperience);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void UpgradeAttackConsumesPointAndIncreasesAttackDamage()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();
        stats.AddExperience(100);

        bool upgraded = stats.TryUpgradeStat(PlayerStatType.Attack);

        Assert.IsTrue(upgraded);
        Assert.AreEqual(0, stats.StatPoints);
        Assert.AreEqual(12, stats.AttackDamage);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void UpgradeWithoutStatPointsDoesNotChangeStats()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();

        bool upgraded = stats.TryUpgradeStat(PlayerStatType.Defense);

        Assert.IsFalse(upgraded);
        Assert.AreEqual(0, stats.StatPoints);
        Assert.AreEqual(5, stats.Defense);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void RestoreVitalsRefillsHealthAndStamina()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();
        stats.TakeDamage(25);
        stats.SpendStamina(40f);

        stats.RestoreVitals();

        Assert.AreEqual(stats.MaxHealth, stats.CurrentHealth);
        Assert.AreEqual(stats.MaxStamina, stats.CurrentStamina);

        Object.DestroyImmediate(player);
    }
}
