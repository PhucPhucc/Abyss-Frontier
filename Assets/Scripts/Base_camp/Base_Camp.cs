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

    private void Start()
    {
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

        if (statScreenUI != null)
        {
            statScreenUI.SetActive(isStatsOpen);
        }

        if (isStatsOpen)
        {
            RestPlayer();
        }
    }

    private void RestPlayer()
    {
        Debug.Log("Player is resting at the Base Camp... HP and Stamina restored!");
        
        // Quái vật chỉ hồi sinh khi người chơi nghỉ ngơi tại Hub (Base Camp)
        // Gọi Event/Function của DungeonManager hoặc HubManager để hồi sinh quái nếu cần
        // Ví dụ:
        // if (HubManager.Instance != null) HubManager.Instance.Rest();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Nhận diện Player thông qua Tag hoặc Component PlayerController
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            isPlayerInRange = true;
            playerTransform = other.transform;

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

            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(false);
            }
            if (statScreenUI != null)
            {
                statScreenUI.SetActive(false);
            }
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

        // Khi mở bảng chỉ số, hiện panel chỉ số dự phòng nếu không có statScreenUI
        if (isStatsOpen && statScreenUI == null)
        {
            Rect boxRect = new Rect(20, 20, 320, 260);
            GUI.Box(boxRect, "=== THÔNG SỐ BẢN THÂN ===");

            GUILayout.BeginArea(new Rect(30, 50, 300, 220));
            
            // Lấy thông tin từ PlayerController nếu có thể
            string staminaInfo = "100 / 100";
            if (playerTransform != null)
            {
                PlayerController controller = playerTransform.GetComponent<PlayerController>();
                if (controller != null)
                {
                    // Lấy các giá trị hiển thị demo
                    staminaInfo = "Hồi phục hoàn toàn";
                }
            }

            GUILayout.Label("<b>Máu (HP):</b> 100 / 100");
            GUILayout.Label("<b>Thể lực (Stamina):</b> " + staminaInfo);
            GUILayout.Label("<b>Sức mạnh (ATK):</b> 10");
            GUILayout.Label("<b>Phòng thủ (DEF):</b> 5");
            GUILayout.Label("<b>Kinh nghiệm (EXP):</b> 0");
            GUILayout.Space(10);
            GUILayout.Label("<i>(Đang nghỉ ngơi tại Hub...)</i>");
            GUILayout.Space(10);
            GUILayout.Label("<color=yellow>Nhấn [E] để đóng bảng chỉ số</color>");

            GUILayout.EndArea();
        }
    }
}
