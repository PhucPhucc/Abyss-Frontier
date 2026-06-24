using UnityEngine;
using UnityEngine.InputSystem;

public class Base_Camp : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("UI Panel display player stats. Toggle on/off on interaction.")]
    [SerializeField] private GameObject statScreenUI;
    
    [Tooltip("UI Prompt text showing 'Press E to Interact'.")]
    [SerializeField] private GameObject interactPromptUI;

    private bool isPlayerInRange = false;
    private bool isStatsOpen = false;
    private Transform playerTransform;
    private PlayerStats playerStats;
    private StatScreenUI statScreen;

    private void Start()
    {
        CacheStatScreen();

        if (statScreenUI != null)
        {
            statScreenUI.SetActive(false);
        }
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInRange)
        {
            // Kiểm tra phím "E" được nhấn 
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                ToggleStats();
            }
        }
    }

    private void ToggleStats()
    {
        isStatsOpen = !isStatsOpen;

        if (isStatsOpen)
        {
            playerStats = EnsurePlayerStats();
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

    private void RestPlayer()
    {
        if (playerStats != null)
        {
            playerStats.RestoreVitals();
        }

        Debug.Log("Player is resting at the Base Camp... HP and Stamina restored!");
        
        // Quái vật chỉ hồi sinh khi người chơi nghỉ ngơi tại Hub (Base Camp)
        // Gọi Event/Function của DungeonManager hoặc HubManager để hồi sinh quái nếu cần
        // Ví dụ:
        // if (HubManager.Instance != null) HubManager.Instance.Rest();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            isPlayerInRange = true;
            playerTransform = other.transform;
            playerStats = EnsurePlayerStats();

            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(true);
            }
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

            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(false);
            }

            CloseStats();
        }
    }

    private void CacheStatScreen()
    {
        if (statScreenUI == null)
        {
            return;
        }

        statScreen = statScreenUI.GetComponent<StatScreenUI>();
        if (statScreen == null)
        {
            statScreen = statScreenUI.AddComponent<StatScreenUI>();
        }
    }

    private void EnsureStatScreen()
    {
        if (statScreen != null)
        {
            return;
        }

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
        if (playerTransform == null)
        {
            return null;
        }

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

    // Giao diện GUI dự phòng hiển thị tạm thời trên màn hình khi chưa kéo thả UI vào Inspector
    private void OnGUI()
    {
        // Khi người chơi lại gần, hiện text hướng dẫn nếu không có interactPromptUI
        if (isPlayerInRange && !isStatsOpen && interactPromptUI == null)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.box);
            promptStyle.fontSize = 16;
            promptStyle.normal.textColor = Color.white;
            promptStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Box(new Rect(Screen.width / 2 - 150, Screen.height - 60, 300, 40), "Nhấn [E] để nghỉ ngơi / Xem chỉ số", promptStyle);
        }

        if (isStatsOpen && statScreenUI == null)
        {
            Rect boxRect = new Rect(20, 20, 320, 260);
            GUI.Box(boxRect, "=== THONG SO BAN THAN ===");

            GUILayout.BeginArea(new Rect(30, 50, 300, 220));

            PlayerStats stats = playerStats != null ? playerStats : EnsurePlayerStats();
            if (stats != null)
            {
                GUILayout.Label($"<b>Mau (HP):</b> {stats.CurrentHealth} / {stats.MaxHealth}");
                GUILayout.Label($"<b>The luc (Stamina):</b> {Mathf.CeilToInt(stats.CurrentStamina)} / {Mathf.CeilToInt(stats.MaxStamina)}");
                GUILayout.Label($"<b>Sat thuong (ATK):</b> {stats.AttackDamage}");
                GUILayout.Label($"<b>Phong thu (DEF):</b> {stats.Defense}");
                GUILayout.Label($"<b>Kinh nghiem (EXP):</b> {stats.CurrentExperience} / {stats.ExperiencePerStatPoint}");
                GUILayout.Label($"<b>Diem nang cap:</b> {stats.StatPoints}");
            }
            else
            {
                GUILayout.Label("<b>Khong tim thay PlayerStats.</b>");
            }

            GUILayout.Space(10);
            GUILayout.Label("<i>(Dang nghi ngoi tai Hub...)</i>");
            GUILayout.Space(10);
            GUILayout.Label("<color=yellow>Nhan [E] de dong bang chi so</color>");

            GUILayout.EndArea();
        }
    }
}
