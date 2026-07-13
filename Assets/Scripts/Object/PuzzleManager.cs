using System.Collections.Generic;
using System.Linq; // Yêu cầu có Linq để dùng SequenceEqual
using UnityEngine;
using UnityEngine.UI; // Thêm thư viện này nếu bạn dùng Text (Hoặc dùng TMPro nếu dùng TextMeshPro)

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Đáp án (Thứ tự đúng)")]
    public List<SwitchType> correctSequence;

    [Header("Giao diện UI")]
    public Text feedbackText; // Kéo thả UI Text thông báo vào đây trên Inspector

    private List<SwitchType> currentInputSequence = new List<SwitchType>();
    private List<PuzzleSwitch> activatedSwitches = new List<PuzzleSwitch>();

    private GameObject door;

    private void Awake()
    {
        Instance = this;
        door = GameObject.FindGameObjectWithTag("Door");
    }

    public void OnSwitchActivated(SwitchType type, PuzzleSwitch pressedSwitch)
    {
        // Chặn không cho nhập thêm nếu đã đạt tối đa số lượt (tránh lỗi nếu người chơi bấm quá nhanh lúc đang reset)
        if (currentInputSequence.Count >= correctSequence.Count) return;

        currentInputSequence.Add(type);
        activatedSwitches.Add(pressedSwitch);

        // --- DEBUG LOG ---
        string currentLog = string.Join(" -> ", currentInputSequence);
        Debug.Log("Thứ tự system ghi nhận hiện tại: " + currentLog);
        // -----------------

        // XÓA BỎ BƯỚC KIỂM TRA TỪNG NÚT Ở ĐÂY

        // CHỈ KIỂM TRA KHI: Đã gạt đủ nút (ví dụ 6 nút)
        if (currentInputSequence.Count == correctSequence.Count)
        {
            CheckFinalSequence();
        }
    }

    private void CheckFinalSequence()
    {
        // SequenceEqual sẽ tự động so sánh từng phần tử theo đúng thứ tự giữa 2 List
        bool isCorrect = currentInputSequence.SequenceEqual(correctSequence);

        if (isCorrect)
        {
            Debug.Log("Giải đố thành công! Mở cửa phòng Boss!");
            if (feedbackText != null) feedbackText.text = "Chính xác! Cửa đã mở.";

            // Gọi hàm mở cửa tại đây
        }
        else
        {
            Debug.Log("Sai mật mã! Reset lại toàn bộ.");
            if (feedbackText != null) feedbackText.text = "Sai rồi! Vui lòng thử lại.";

            ResetPuzzle();
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