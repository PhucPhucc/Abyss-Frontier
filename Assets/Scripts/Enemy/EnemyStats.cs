using UnityEngine;

public enum EnemyType
{
    Plant,
    Slime,
    Orc,
    Vampire,
    Boss
}

public enum EnemyLevel
{
    Level1 = 1,
    Level2 = 2,
    Level3 = 3
}

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Abyss Frontier/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Enemy Identity")]
    public string enemyName;
    public EnemyType enemyType;

    [Header("Base Stats (Level 1)")]
    public int baseHP = 30;
    public int baseATK = 5;
    public int baseDEF = 2;
    public float baseSpeed = 2f;
    public int baseExpReward = 10;

    [Header("Scaling Per Level")]
    public float hpScale = 1.6f;
    public float atkScale = 1.5f;
    public float defScale = 1.3f;
    public float speedScale = 1.1f;
    public float expScale = 1.8f;

    public int GetHP(int level) => Mathf.RoundToInt(baseHP * Mathf.Pow(hpScale, level - 1));
    public int GetATK(int level) => Mathf.RoundToInt(baseATK * Mathf.Pow(atkScale, level - 1));
    public int GetDEF(int level) => Mathf.RoundToInt(baseDEF * Mathf.Pow(defScale, level - 1));
    public float GetSpeed(int level) => baseSpeed * Mathf.Pow(speedScale, level - 1);
    public int GetExpReward(int level) => Mathf.RoundToInt(baseExpReward * Mathf.Pow(expScale, level - 1));
}
