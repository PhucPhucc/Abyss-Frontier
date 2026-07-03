using UnityEngine;

public class ItemCollect : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem đối tượng va chạm có tag là "Player" không
        if (collision.CompareTag("SpawnPointer"))
        {
            // Thêm logic cộng điểm vào GameManager của bạn ở đây
            // GameManager.instance.AddScore(scoreValue);

            Debug.Log("Coin Collected!");
            Destroy(gameObject); // Xóa vật phẩm khỏi cảnh
        }
    }
}