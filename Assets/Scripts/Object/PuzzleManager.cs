using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class PuzzleManager : NetworkBehaviour
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

    private bool IsSingleplayer => Runner == null || Runner.GameMode == GameMode.Single;

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

    /// <summary>
    /// Được gọi bởi PuzzleSwitch khi switch được kích hoạt.
    /// Trong multiplayer, hàm này chỉ chạy trên Host (StateAuthority).
    /// </summary>
    public void OnSwitchActivated(SwitchType type, PuzzleSwitch pressedSwitch)
    {
        // Multiplayer: chỉ Host xử lý logic puzzle
        if (!IsSingleplayer && !Object.HasStateAuthority)
            return;

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

    // ── Puzzle Success ──────────────────────────────────────────────────────────

    // Hàm xử lý khi giải ĐÚNG (Tách ra để dùng chung cho cả Test lẫn Gameplay thực)
    private void TriggerPuzzleSuccess()
    {
        if (IsSingleplayer)
        {
            ApplySuccessLocal();
            return;
        }

        // Multiplayer: Host broadcast success cho tất cả clients
        if (Object.HasStateAuthority)
        {
            ApplySuccessLocal(); // Host tự apply
            RPC_BroadcastSuccess();
        }
    }

    /// <summary>Host broadcast kết quả thành công cho tất cả clients.</summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastSuccess()
    {
        // Host đã apply trong TriggerPuzzleSuccess, chỉ apply cho clients
        if (!Object.HasStateAuthority)
            ApplySuccessLocal();
    }

    private void ApplySuccessLocal()
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

    // ── Puzzle Failure ──────────────────────────────────────────────────────────

    // Hàm xử lý khi giải SAI (Tách ra để dùng chung cho cả Test lẫn Gameplay thực)
    private void TriggerPuzzleFailure()
    {
        if (IsSingleplayer)
        {
            ApplyFailureLocal();
            ResetPuzzle();
            return;
        }

        // Multiplayer: Host broadcast failure cho tất cả clients
        if (Object.HasStateAuthority)
        {
            ApplyFailureLocal(); // Host tự apply (including spawn penalty)
            RPC_BroadcastFailure();
            ResetPuzzle();
        }
    }

    /// <summary>Host broadcast kết quả thất bại cho tất cả clients.</summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastFailure()
    {
        // Clients chỉ cập nhật UI — penalty spawn do Host handle riêng
        if (!Object.HasStateAuthority)
        {
            if (feedbackText != null)
                feedbackText.text = "Wrong! Enemies are approaching!";
        }
    }

    private void ApplyFailureLocal()
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
    }

    // ── Reset ───────────────────────────────────────────────────────────────────

    private void ResetPuzzle()
    {
        currentInputSequence.Clear();
        foreach (var sw in activatedSwitches)
        {
            sw.ResetSwitch(); // PuzzleSwitch.ResetSwitch() đã handle network sync
        }
        activatedSwitches.Clear();
        Debug.Log("[PuzzleManager] Đã reset lại trạng thái các công tắc.");
    }
}