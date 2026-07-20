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

    private void Awake()
    {
        Instance = this;
        door = GameObject.FindGameObjectWithTag("Door");
    }

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

    public void OnSwitchActivated(SwitchType type, PuzzleSwitch pressedSwitch)
    {
        if (!Object.HasStateAuthority) return;

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
            TriggerPuzzleSuccess();
        else
            TriggerPuzzleFailure();
    }

    private void TriggerPuzzleSuccess()
    {
        Debug.Log("<color=green>Giải đố thành công! Mở cửa phòng Boss!</color>");

        if (door != null)
        {
            DoorController doorController = door.GetComponent<DoorController>();
            if (doorController != null)
                doorController.OpenDoor();
            else
            {
                Debug.LogWarning("[PuzzleManager] Không tìm thấy DoorController trên GameObject tagged 'Door'.");
                door.SetActive(false);
            }
        }

        RPC_BroadcastFeedback("Bingo! The door is open.");
    }

    private void TriggerPuzzleFailure()
    {
        Debug.Log("<color=red>Sai mật mã! Chuẩn bị kích hoạt phạt spawn quái và reset.</color>");

        if (penaltySpawner != null)
        {
            penaltySpawner.SpawnEnemies();
        }
        else
        {
            Debug.LogError("[PuzzleManager] LỖI: Biến 'penaltySpawner' chưa được gán trên Inspector!");
        }

        ResetPuzzle();

        RPC_BroadcastFeedback("Wrong! Enemies are approaching!");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
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
