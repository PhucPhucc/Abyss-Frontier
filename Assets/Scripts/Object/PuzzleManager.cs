using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Answer")]
    public List<SwitchType> correctSequence;

    [Header("UI Interface")]
    public TextMeshProUGUI feedbackText;

    [Header("Enemy Spawner")]
    public FixedPointsSpawner penaltySpawner;

    private List<SwitchType> currentInputSequence = new List<SwitchType>();
    private List<PuzzleSwitch> activatedSwitches = new List<PuzzleSwitch>();
    private GameObject door;

    private void Awake()
    {
        Instance = this;
        door = GameObject.FindGameObjectWithTag("Door");
    }

    // ==========================================
    // KHU VỰC TEST NHANH TRÊN INSPECTOR
    // ==========================================

    [ContextMenu("Test: Giải ĐÚNG (Success)")]
    public void TestCorrectPuzzle()
    {
        Debug.LogWarning("[TEST] Kích hoạt thủ công trạng thái GIẢI ĐỐ ĐÚNG!");
        TriggerPuzzleSuccess();
    }

    [ContextMenu("Test: Giải SAI (Failure)")]
    public void TestWrongPuzzle()
    {
        Debug.LogWarning("[TEST] Kích hoạt thủ công trạng thái GIẢI ĐỐ SAI!");
        TriggerPuzzleFailure();
    }

    // ==========================================
    // LOGIC GAME CHÍNH
    // ==========================================

    public void OnSwitchActivated(SwitchType type, PuzzleSwitch pressedSwitch)
    {
        if (currentInputSequence.Count >= correctSequence.Count) return;

        currentInputSequence.Add(type);
        activatedSwitches.Add(pressedSwitch);

        if (currentInputSequence.Count == correctSequence.Count)
        {
            CheckFinalSequence();
        }
    }

    private void CheckFinalSequence()
    {
        bool isCorrect = currentInputSequence.SequenceEqual(correctSequence);
        if (isCorrect)
        {
            TriggerPuzzleSuccess();
        }
        else
        {
            TriggerPuzzleFailure();
        }
    }

    // Hàm xử lý khi giải ĐÚNG (Tách ra để dùng chung cho cả Test lẫn Gameplay thực)
    private void TriggerPuzzleSuccess()
    {
        Debug.Log("<color=green>Giải đố thành công! Mở cửa phòng Boss!</color>");
        
        if (feedbackText != null) 
            feedbackText.text = "Bingo! The door is open.";
            
        // Gọi hàm mở cửa tại đây
        if (door != null)
        {
            DoorController doorController = door.GetComponent<DoorController>();
            if (doorController != null)
            {
                doorController.OpenDoor();
            }
            else
            {
                Debug.LogWarning("[PuzzleManager] Không tìm thấy component DoorController trên GameObject tagged 'Door'. Đang tự động ẩn GameObject để giải phóng lối đi!");
                door.SetActive(false);
            }
        }
    }

    // Hàm xử lý khi giải SAI (Tách ra để dùng chung cho cả Test lẫn Gameplay thực)
    private void TriggerPuzzleFailure()
    {
        Debug.Log("<color=red>Sai mật mã! Chuẩn bị kích hoạt phạt spawn quái và reset.</color>");
        
        if (feedbackText != null) 
            feedbackText.text = "Wrong! Enemies are approaching!";

        if (penaltySpawner != null)
        {
            Debug.Log("[PuzzleManager] Đang gọi hàm SpawnEnemies() từ FixedPointsSpawner...");
            penaltySpawner.SpawnEnemies();
        }
        else
        {
            Debug.LogError("[PuzzleManager] LỖI: Biến 'penaltySpawner' chưa được gán trên Inspector!");
        }

        ResetPuzzle();
    }

    private void ResetPuzzle()
    {
        currentInputSequence.Clear();
        foreach (var sw in activatedSwitches)
        {
            sw.ResetSwitch();
        }
        activatedSwitches.Clear();
        Debug.Log("[PuzzleManager] Đã reset lại trạng thái các công tắc.");
    }
}