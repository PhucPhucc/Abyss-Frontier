using UnityEngine;
using UnityEngine.InputSystem;

public class Base_Camp : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject statScreenUI;
    [SerializeField] private GameObject interactPromptUI;

    private bool isPlayerInRange = false;
    private bool isStatsOpen = false;
    private Transform playerTransform;
    private PlayerStats playerStats;
    private PlayerHealth playerHealth;
    private PlayerController playerController;

    private static readonly string[] StatNames = {
        "Sức mạnh (Strength)",
        "Khéo léo (Dexterity)",
        "Sinh lực (Vitality)",
        "Nhanh nhẹn (Agility)",
        "Bền bỉ (Endurance)",
        "Trí lực (Intelligence)"
    };

    private static readonly string[] StatEffects = {
        "ATK: +2 mỗi điểm",
        "Né: +2% mỗi điểm",
        "Máu: +20 mỗi điểm",
        "Tốc độ: +0.15 mỗi điểm",
        "Giảm hao ST: +10% mỗi điểm",
        "EXP nhận: +10% mỗi điểm"
    };

    private static readonly StatType[] StatTypes = {
        StatType.Strength,
        StatType.Dexterity,
        StatType.Vitality,
        StatType.Agility,
        StatType.Endurance,
        StatType.Intelligence
    };

    private void Start()
    {
        if (statScreenUI != null) statScreenUI.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleStats();
        }

        if (isStatsOpen && playerStats != null && playerStats.AvailableStatPoints > 0 && Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) AllocateStat(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) AllocateStat(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) AllocateStat(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) AllocateStat(3);
            if (Keyboard.current.digit5Key.wasPressedThisFrame) AllocateStat(4);
            if (Keyboard.current.digit6Key.wasPressedThisFrame) AllocateStat(5);
        }
    }

    private void AllocateStat(int index)
    {
        if (playerStats == null || playerStats.AvailableStatPoints <= 0) return;

        playerStats.AllocateStat(StatTypes[index]);
        if (playerController != null)
            playerController.RefreshStats();
    }

    private void ToggleStats()
    {
        isStatsOpen = !isStatsOpen;

        if (statScreenUI != null)
            statScreenUI.SetActive(isStatsOpen);

        if (isStatsOpen)
        {
            if (playerTransform != null)
            {
                playerStats = playerTransform.GetComponent<PlayerStats>();
                playerHealth = playerTransform.GetComponent<PlayerHealth>();
                playerController = playerTransform.GetComponent<PlayerController>();
            }
            RestPlayer();
        }
    }

    private void RestPlayer()
    {
        Debug.Log("Player is resting at the Base Camp... HP restored!");
        if (playerHealth != null) playerHealth.RestoreFullHealth();
        if (playerController != null) playerController.RefreshStats();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            isPlayerInRange = true;
            playerTransform = other.transform;

            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            isPlayerInRange = false;
            isStatsOpen = false;
            playerTransform = null;
            playerStats = null;
            playerHealth = null;
            playerController = null;

            if (interactPromptUI != null) interactPromptUI.SetActive(false);
            if (statScreenUI != null) statScreenUI.SetActive(false);
        }
    }

    private void OnGUI()
    {
        if (isPlayerInRange && !isStatsOpen && interactPromptUI == null)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.box);
            promptStyle.fontSize = 14;
            promptStyle.normal.textColor = Color.white;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Box(new Rect(Screen.width / 2 - 160, Screen.height - 60, 320, 40), "Nhấn [E] để nghỉ ngơi / Xem chỉ số", promptStyle);
        }

        if (isStatsOpen && statScreenUI == null && playerStats != null)
        {
            int[] statValues = {
                playerStats.Strength,
                playerStats.Dexterity,
                playerStats.Vitality,
                playerStats.Agility,
                playerStats.Endurance,
                playerStats.Intelligence
            };

            int points = playerStats.AvailableStatPoints;
            int hp = playerHealth != null ? playerHealth.CurrentHealth : 0;
            int maxHp = playerStats.MaxHealth;
            int atk = playerStats.AttackDamage;
            float dodge = playerStats.DodgeChance * 100;
            float speed = playerStats.MoveSpeed;
            float expMul = playerStats.ExpMultiplier;

            float panelHeight = 420f;
            if (points > 0) panelHeight += 30f;

            Rect boxRect = new Rect(20, 20, 460, panelHeight);
            GUI.Box(boxRect, "=== BASE CAMP - STAT SCREEN ===");

            GUILayout.BeginArea(new Rect(30, 40, 440, panelHeight - 20));

            GUILayout.Label($"Level: {playerStats.Level}  |  EXP: {playerStats.CurrentExp}/{playerStats.ExpToNextLevel}");
            GUILayout.Label($"HP: {hp}/{maxHp}");
            GUILayout.Label($"ATK: {atk}  |  Dodge: {dodge:F0}%  |  Speed: {speed:F2}  |  EXP Multi: x{expMul:F1}");
            GUILayout.Space(10);

            if (points > 0)
            {
                GUIStyle pointsStyle = new GUIStyle(GUI.skin.label);
                pointsStyle.normal.textColor = Color.green;
                pointsStyle.fontStyle = FontStyle.Bold;
                GUILayout.Label($"Available Stat Points: {points}", pointsStyle);
                GUILayout.Space(5);
            }

            for (int i = 0; i < 6; i++)
            {
                string key = (i + 1).ToString();
                GUILayout.BeginHorizontal();

                string label = points > 0
                    ? $"[{key}] {StatNames[i]}: {statValues[i]}"
                    : $"  {StatNames[i]}: {statValues[i]}";
                GUILayout.Label(label, GUILayout.Width(260));
                GUILayout.Label(StatEffects[i], GUILayout.Width(160));

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("Nhấn [E] để đóng");
            if (points > 0)
            {
                GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
                hintStyle.normal.textColor = Color.yellow;
                GUILayout.Label("Nhấn [1]-[6] để tăng chỉ số tương ứng", hintStyle);
            }

            GUILayout.EndArea();
        }
    }
}
