using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyLevel enemyLevel = EnemyLevel.Level1;
    [SerializeField] private EnemyStats statsDefinition;

    private int currentHP;
    private int def;
    private int maxHP;

    private EnemyAI enemyAI;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public int DEF => def;
    public EnemyLevel EnemyLevel => enemyLevel;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        if (statsDefinition != null)
        {
            int level = (int)enemyLevel;
            maxHP = statsDefinition.GetHP(level);
            def = statsDefinition.GetDEF(level);
            currentHP = maxHP;

            if (enemyAI != null)
            {
                enemyAI.SetStatsFromDefinition(statsDefinition, level);
            }
        }
        else
        {
            maxHP = 30;
            def = 0;
            currentHP = maxHP;
        }
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(1, damage - def);
        currentHP -= actualDamage;
        currentHP = Mathf.Max(0, currentHP);

        if (enemyAI != null)
        {
            enemyAI.TriggerHurtAnimation();
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (statsDefinition != null)
        {
            int expReward = statsDefinition.GetExpReward((int)enemyLevel);
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerStats ps = player.GetComponent<PlayerStats>();
                if (ps != null)
                {
                    ps.AddExp(expReward);
                }
            }
        }
        Destroy(gameObject);
    }
}
