using UnityEngine;

public class ClueReader : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject clueUIPanel;
    
    [Tooltip("The maximum distance for reading a book.")]
    public float interactionRange = 2f;

    private Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Not found Player");
        }
    }

    // Hàm này chạy khi click chuột vào Collider của cuốn sách
    private void OnMouseDown()
    {
        // 1. Kiểm tra an toàn xem đã gán Player chưa
        if (player == null)
        {
            Debug.LogWarning("You forgot to drag the Player object into the ClueReader script");
            return;
        }

        // 2. Tính khoảng cách giữa Sách (transform.position) và Người chơi (player.position)
        float distance = Vector2.Distance(transform.position, player.position);

        // 3. Nếu khoảng cách nhỏ hơn hoặc bằng giới hạn cho phép -> Mở sách
        if (distance <= interactionRange)
        {
            if (clueUIPanel != null)
            {
                clueUIPanel.SetActive(true);
            }
        }
        else
        {
            // Nếu đứng quá xa
            Debug.Log("You're too far away; get closer to the book");
        }
    }

    // (Bonus) Hàm này vẽ một vòng tròn màu vàng trong cửa sổ Scene để bạn dễ căn chỉnh khoảng cách
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}