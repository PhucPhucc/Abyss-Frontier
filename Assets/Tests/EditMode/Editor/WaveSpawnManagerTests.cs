using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WaveSpawnManagerTests
{
    [UnityTest]
    public IEnumerator FinalWaveRemovesLivingEnemiesSpawnsBossAndShowsVictoryAfterBossDies()
    {
        float originalTimeScale = Time.timeScale;
        GameObject managerObject = null;
        GameObject regularEnemy = null;
        GameObject bossPrefab = null;
        GameObject victoryUiObject = null;
        GameObject victoryPanel = null;

        try
        {
            managerObject = new GameObject("Wave Manager");
            WaveSpawnManager manager = managerObject.AddComponent<WaveSpawnManager>();
            SetPrivateField(manager, "spawnBossAfterFinalWave", true);

            regularEnemy = new GameObject("Regular Enemy");
            regularEnemy.AddComponent<EnemyHealth>();

            bossPrefab = new GameObject("Wave Test Boss Prefab");
            EnemyHealth bossPrefabHealth = bossPrefab.AddComponent<EnemyHealth>();
            SetPrivateField(bossPrefabHealth, "respawnOnDeath", false);
            bossPrefab.SetActive(false);
            SetPrivateField(manager, "bossSlimePrefab", bossPrefab);

            victoryPanel = new GameObject("Victory Panel");
            victoryPanel.SetActive(false);
            victoryUiObject = new GameObject("Boss Victory UI");
            BossVictoryUI victoryUi = victoryUiObject.AddComponent<BossVictoryUI>();
            SetPrivateField(victoryUi, "winPanel", victoryPanel);

            manager.StartWave(0);
            yield return null;

            Assert.IsTrue(regularEnemy == null, "Final wave should remove the living scene enemies.");

            EnemyHealth spawnedBossHealth = FindInactiveOrActiveEnemy("Wave Test Boss Prefab(Clone)");
            Assert.IsNotNull(spawnedBossHealth);
            GameObject spawnedBoss = spawnedBossHealth.gameObject;
            spawnedBoss.SetActive(true);
            Assert.AreEqual(manager.transform.position, spawnedBoss.transform.position);
            LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode!"));
            spawnedBossHealth.TakeDamage(999, Vector2.right);

            Assert.IsTrue(victoryPanel.activeSelf, "Defeating the spawned boss should complete the floor.");
        }
        finally
        {
            Time.timeScale = originalTimeScale;
            DestroyImmediateIfPresent(managerObject);
            DestroyImmediateIfPresent(regularEnemy);
            DestroyImmediateIfPresent(bossPrefab);
            DestroyImmediateIfPresent(victoryUiObject);
            DestroyImmediateIfPresent(victoryPanel);
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"Expected serialized field '{fieldName}' on {target.GetType().Name}.");
        field.SetValue(target, value);
    }

    private static void DestroyImmediateIfPresent(Object gameObject)
    {
        if (gameObject != null)
            Object.DestroyImmediate(gameObject);
    }

    private static EnemyHealth FindInactiveOrActiveEnemy(string objectName)
    {
        EnemyHealth[] enemyHealths = Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (EnemyHealth enemyHealth in enemyHealths)
        {
            if (enemyHealth.gameObject.name == objectName)
                return enemyHealth;
        }

        return null;
    }
}
