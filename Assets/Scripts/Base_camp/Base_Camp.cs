using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Khu vực Base Camp (Hub) — nơi Player nghỉ ngơi, hồi máu và phân bổ stat points.
/// Chỉ có thể mở Stat Screen khi ở trong vùng này.
/// </summary>
public class Base_Camp : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("UI Panel display player stats. Toggle on/off on interaction.")]
    [SerializeField] private GameObject statScreenUI;      // UI Stat Screen (nếu dùng Canvas, null nếu dùng OnGUI)
    [Tooltip("UI Prompt text showing 'Press E to Interact'.")]
    [SerializeField] private GameObject interactPromptUI;  // UI nhắc nhấn [E] (nếu dùng Canvas)

    private bool isPlayerInRange = false; // Player có đang trong vùng Base Camp không?
    private bool isStatsOpen = false;     // Stat Screen có đang mở không?
    private Transform playerTransform;
    private PlayerStats playerStats;
    private PlayerHealth playerHealth;
    private PlayerController playerController;
    private StatScreenUI statScreen;      // Caching StatScreenUI component

    // Tên hiển thị cho từng stat (tiếng Việt)
    private static readonly string[] StatNames = {
        "Sức mạnh (Strength)",
        "Khéo léo (Dexterity)",
        "Sinh lực (Vitality)",
        "Nhanh nhẹn (Agility)",
        "Bền bỉ (Endurance)",
        "Trí lực (Intelligence)"
    };

    // Mô tả hiệu ứng từng stat
    private static readonly string[] StatEffects = {
        "ATK: +2 mỗi điểm",
        "Né: +2% mỗi điểm",
        "Máu: +20 mỗi điểm",
        "Tốc độ: +0.15 mỗi điểm",
        "Giảm hao ST: +10% mỗi điểm",
        "EXP nhận: +10% mỗi điểm"
    };

    // Ánh xạ index → StatType
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
        CacheStatScreen();

        // Ẩn các UI khi bắt đầu
        if (statScreenUI != null) statScreenUI.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        // Nhấn [E] để mở/đóng Stat Screen
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleStats();
        }

        // Nếu Stat Screen đang mở và có điểm stat: nhấn [1]-[6] để phân bổ
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

    /// <summary>
    /// Phân bổ điểm stat tương ứng với phím số được nhấn.
    /// </summary>
    private void AllocateStat(int index)
    {
        if (playerStats == null || playerStats.AvailableStatPoints <= 0) return;

        playerStats.AllocateStat(StatTypes[index]);
        if (playerController != null)
            playerController.RefreshStats();
        
        // Cập nhật lại UI Panel nếu đang mở
        if (statScreen != null)
        {
            statScreen.Refresh();
        }
    }

    /// <summary>
    /// Mở hoặc đóng Stat Screen. Khi mở, Player được hồi đầy máu (nghỉ ngơi).
    /// </summary>
    private void ToggleStats()
    {
        isStatsOpen = !isStatsOpen;

        if (isStatsOpen)
        {
            // Lấy component từ Player Transform
            if (playerTransform != null)
            {
                playerStats = playerTransform.GetComponent<PlayerStats>();
                playerHealth = playerTransform.GetComponent<PlayerHealth>();
                playerController = playerTransform.GetComponent<PlayerController>();
            }
            else
            {
                playerStats = EnsurePlayerStats();
            }

            RestPlayer();
            EnsureStatScreen();

            if (statScreen != null)
            {
                statScreen.Open(playerStats);
            }
            else if (statScreenUI != null)
            {
                statScreenUI.SetActive(true);
            }
        }
        else
        {
            CloseStats();
        }
    }

    /// <summary>
    /// Hồi đầy máu và refresh stats khi Player nghỉ ngơi.
    /// </summary>
    private void RestPlayer()
    {
        if (playerStats != null)
        {
            playerStats.RestoreVitals();
        }
        else if (playerHealth != null)
        {
            playerHealth.RestoreFullHealth();
        }

        if (playerController != null)
        {
            playerController.RefreshStats();
        }

        // Hub-only enemy respawn (T-76): revive queued enemies killed in this dungeon run.
        EnemyRespawnRunner.RespawnAllAtHub();
        EnemyHealth.KilledEnemyIds.Clear();

        Debug.Log("Player is resting at the Base Camp... HP and Stamina restored!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            isPlayerInRange = true;
            playerTransform = other.transform;
            playerStats = EnsurePlayerStats();

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

            CloseStats();
        }
    }

    private void CacheStatScreen()
    {
        if (statScreenUI == null) return;

        statScreen = statScreenUI.GetComponent<StatScreenUI>();
        if (statScreen == null)
        {
            statScreen = statScreenUI.AddComponent<StatScreenUI>();
        }
    }

    private void EnsureStatScreen()
    {
        if (statScreen != null) return;

        if (statScreenUI != null)
        {
            CacheStatScreen();
            return;
        }

        statScreen = StatScreenUI.CreateRuntimeScreen();
        statScreenUI = statScreen.gameObject;
    }

    private PlayerStats EnsurePlayerStats()
    {
        if (playerTransform == null) return null;

        PlayerStats stats = playerTransform.GetComponent<PlayerStats>();
        if (stats == null)
        {
            stats = playerTransform.gameObject.AddComponent<PlayerStats>();
        }
        return stats;
    }

    private void CloseStats()
    {
        if (statScreen != null)
        {
            statScreen.Close();
        }
        else if (statScreenUI != null)
        {
            statScreenUI.SetActive(false);
        }
    }

    /// <summary>
    /// Vẽ UI bằng GUI (dùng khi không có Canvas UI).
    /// </summary>
    private void DrawWideStatScreen()
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
        float panelWidth = Screen.width * 0.8f;
        float panelHeight = Screen.height * 0.8f;
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        GUI.Box(panelRect, "BASE CAMP - STAT SCREEN");

        float padding = Mathf.Max(18f, panelWidth * 0.025f);
        float titleHeight = 34f;
        float contentTop = panelRect.y + padding + titleHeight;
        float contentHeight = panelRect.height - (padding * 2f) - titleHeight;
        float gap = Mathf.Max(18f, panelWidth * 0.025f);
        float leftWidth = (panelRect.width - (padding * 2f) - gap) * 0.58f;
        float rightWidth = (panelRect.width - (padding * 2f) - gap) - leftWidth;

        Rect leftRect = new Rect(panelRect.x + padding, contentTop, leftWidth, contentHeight);
        Rect rightRect = new Rect(leftRect.xMax + gap, contentTop, rightWidth, contentHeight);

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        headerStyle.normal.textColor = Color.white;

        GUIStyle statStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15
        };
        statStyle.normal.textColor = Color.white;

        GUIStyle pointsStyle = new GUIStyle(statStyle)
        {
            fontStyle = FontStyle.Bold
        };
        pointsStyle.normal.textColor = points > 0 ? Color.green : Color.white;

        GUILayout.BeginArea(leftRect);
        GUILayout.Label("THONG TIN CHIEN DAU", headerStyle);
        GUILayout.Space(8f);
        GUILayout.Label($"Level: {playerStats.Level}  |  EXP: {playerStats.CurrentExp}/{playerStats.ExpToNextLevel}", statStyle);
        GUILayout.Label($"HP: {playerStats.CurrentHealth}/{playerStats.MaxHealth}", statStyle);
        GUILayout.Label($"The luc: {Mathf.CeilToInt(playerStats.CurrentStamina)}/{Mathf.CeilToInt(playerStats.MaxStamina)}", statStyle);
        GUILayout.Label($"ATK: {playerStats.AttackDamage}  |  Dodge: {playerStats.DodgeChance * 100:F0}%  |  Speed: {playerStats.MoveSpeed:F2}", statStyle);
        GUILayout.Label($"EXP Multi: x{playerStats.ExpMultiplier:F1}", statStyle);
        GUILayout.Label($"Available Stat Points: {points}", pointsStyle);
        GUILayout.Space(18f);
        GUILayout.Label("CHI SO CO BAN", headerStyle);
        GUILayout.Space(8f);

        for (int i = 0; i < StatNames.Length; i++)
        {
            GUILayout.Label($"{StatNames[i]}: {statValues[i]}    {StatEffects[i]}", statStyle);
        }

        GUILayout.EndArea();

        GUILayout.BeginArea(rightRect);
        GUILayout.Label("NANG CAP CHI SO", headerStyle);
        GUILayout.Space(12f);

        GUI.enabled = points > 0;
        for (int i = 0; i < StatNames.Length; i++)
        {
            if (GUILayout.Button($"+ {StatNames[i]}", GUILayout.Height(42f)))
            {
                AllocateStat(i);
            }
            GUILayout.Space(8f);
        }
        GUI.enabled = true;

        GUILayout.FlexibleSpace();
        GUILayout.Label(points > 0 ? "Chon nut de nang cap chi so." : "Can them EXP de co diem nang cap.", statStyle);
        GUILayout.Label("Nhan [E] de dong", statStyle);
        GUILayout.EndArea();
    }

    private void OnGUI()
    {
        // Prompt nhấn [E] khi Player trong vùng
        if (isPlayerInRange && !isStatsOpen && interactPromptUI == null)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.box);
            promptStyle.fontSize = 14;
            promptStyle.normal.textColor = Color.white;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Box(new Rect(Screen.width / 2 - 160, Screen.height - 60, 320, 40), "Nhấn [E] để nghỉ ngơi / Xem chỉ số", promptStyle);
        }

        // Stat Screen (dùng GUI fallback)
        if (isStatsOpen && statScreenUI == null && playerStats != null)
        {
            DrawWideStatScreen();
            return;
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
            int hp = playerStats.CurrentHealth;
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
