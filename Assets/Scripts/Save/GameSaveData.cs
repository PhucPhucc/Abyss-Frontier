using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public string sceneName;
    public PlayerSaveData player;
    public List<EnemySaveData> enemies;
    public List<string> killedEnemyIds;
    public List<string> unlockedFloors;
    public string saveTime;
    public int characterIndex;
}

[Serializable]
public class PlayerSaveData
{
    public float posX, posY;
    public int level, currentExp, expToNextLevel;
    public int availableStatPoints;
    public int strength, dexterity, vitality, agility, endurance, intelligence;
    public int currentHealth, maxHealth;
    public float currentStamina, maxStamina;
    public string currentScene;
}

[Serializable]
public class EnemySaveData
{
    public string saveId;
    public bool isDead;
    public float posX, posY;
    public int currentHealth;
}
