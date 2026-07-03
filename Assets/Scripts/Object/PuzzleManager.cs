using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Đáp án (Thứ tự đúng)")]
    public List<SwitchType> correctSequence;

    private List<SwitchType> currentInputSequence = new List<SwitchType>();
    private List<PuzzleSwitch> activatedSwitches = new List<PuzzleSwitch>();

    private void Awake()
    {
        Instance = this;
    }

    public void OnSwitchActivated(SwitchType type, PuzzleSwitch pressedSwitch)
    {
        currentInputSequence.Add(type);
        activatedSwitches.Add(pressedSwitch);

        int currentIndex = currentInputSequence.Count - 1;

        // KIỂM TRA: Nút vừa gạt có đúng vị trí trong chuỗi đáp án không?
        if (currentInputSequence[currentIndex] != correctSequence[currentIndex])
        {
            Debug.Log("Sai mật mã! Reset lại toàn bộ.");
            ResetPuzzle();
            return;
        }

        // KIỂM TRA: Đã gạt đủ 6 nút chưa?
        if (currentInputSequence.Count == correctSequence.Count)
        {
            Debug.Log("Giải đố thành công! Mở cửa phòng Boss!");
            // Gọi hàm mở cửa tại đây
        }
    }

    private void ResetPuzzle()
    {
        currentInputSequence.Clear();

        // Lặp qua tất cả các công tắc đã gạt và bắt chúng nảy lên lại
        foreach (var sw in activatedSwitches)
        {
            sw.ResetSwitch();
        }
        activatedSwitches.Clear();
    }
}