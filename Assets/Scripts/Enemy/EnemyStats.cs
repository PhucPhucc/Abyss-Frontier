using UnityEngine;

/// <summary>
/// Các loại Enemy trong game.
/// </summary>
public enum EnemyType
{
    Plant,    // Tầng 1
    Slime,    // Tầng 2–3
    Orc,      // Tầng 3–4
    Vampire,  // Tầng 4
    Boss      // Tầng 5
}

/// <summary>
/// Cấp độ của Enemy (3 cấp cho mỗi loại).
/// </summary>
public enum EnemyLevel
{
    Level1 = 1,
    Level2 = 2,
    Level3 = 3
}

/// <summary>
/// ScriptableObject định nghĩa chỉ số gốc và hệ số tăng trưởng theo cấp cho từng loại Enemy.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Abyss Frontier/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Enemy Identity")]
    public string enemyName;   // Tên hiển thị
    public EnemyType enemyType; // Loại enemy

    [Header("Base Stats (Level 1)")]
    public int baseHP = 30;          // Máu cơ bản
    public int baseATK = 5;          // Tấn công cơ bản
    public int baseDEF = 2;          // Phòng thủ cơ bản
    public float baseSpeed = 2f;     // Tốc độ cơ bản
    public int baseExpReward = 10;   // EXP thưởng cơ bản

    [Header("Scaling Per Level")]
    public float hpScale = 1.6f;   // Hệ số tăng máu mỗi cấp
    public float atkScale = 1.5f;  // Hệ số tăng ATK mỗi cấp
    public float defScale = 1.3f;  // Hệ số tăng DEF mỗi cấp
    public float speedScale = 1.1f;// Hệ số tăng tốc độ mỗi cấp
    public float expScale = 1.8f;  // Hệ số tăng EXP mỗi cấp

    /// <summary>Tính máu theo cấp: baseHP * hpScale^(level-1)</summary>
    public int GetHP(int level) => Mathf.RoundToInt(baseHP * Mathf.Pow(hpScale, level - 1));
    /// <summary>Tính ATK theo cấp: baseATK * atkScale^(level-1)</summary>
    public int GetATK(int level) => Mathf.RoundToInt(baseATK * Mathf.Pow(atkScale, level - 1));
    /// <summary>Tính DEF theo cấp: baseDEF * defScale^(level-1)</summary>
    public int GetDEF(int level) => Mathf.RoundToInt(baseDEF * Mathf.Pow(defScale, level - 1));
    /// <summary>Tính tốc độ theo cấp: baseSpeed * speedScale^(level-1)</summary>
    public float GetSpeed(int level) => baseSpeed * Mathf.Pow(speedScale, level - 1);
    /// <summary>Tính EXP thưởng theo cấp: baseExpReward * expScale^(level-1)</summary>
    public int GetExpReward(int level) => Mathf.RoundToInt(baseExpReward * Mathf.Pow(expScale, level - 1));
}
