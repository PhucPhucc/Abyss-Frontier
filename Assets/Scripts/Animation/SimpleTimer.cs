using UnityEngine;
using TMPro; // Thư viện dùng cho TextMeshPro

public class SimpleTimer : MonoBehaviour
{
    [Header("Count down time (second)")]
    public float maxTime = 300f;
    private float currentTime;

    [Header("CountdownText")]
    public TextMeshProUGUI timeText;

    void Start()
    {
        // Gán thời gian bắt đầu
        currentTime = maxTime;
    }

    void Update()
    {
        if (currentTime > 0)
        {
            // Trừ dần thời gian thực
            currentTime -= Time.deltaTime;

            // Tính số phút và giây
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);

            // Cập nhật text hiển thị (định dạng 00:00)
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            // Ép về 0 khi hết giờ tránh hiển thị số âm
            currentTime = 0;
            timeText.text = "00:00";

            // Gọi hàm xử lý hết giờ
            TimeIsUp();
        }
    }

    void TimeIsUp()
    {
        Debug.Log("Hết giờ rồi!");
        // Viết code xử lý game over hoặc đổi map ở đây
    }
}