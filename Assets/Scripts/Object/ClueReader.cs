using UnityEngine;
using UnityEngine.InputSystem; // 1. Bắt buộc thêm dòng này

[RequireComponent(typeof(InteractableTrigger))]
public class ClueReader : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject clueUIPanel;
    public float interactionRange = 2f;
    private Transform player;

    private void Update()
    {
        // 2. Sửa lại cách bắt phím E theo hệ thống mới
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryOpenBook();
        }
    }

    private void TryOpenBook()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= interactionRange)
        {
            if (clueUIPanel != null) clueUIPanel.SetActive(true);
        }
        else
        {
            Debug.Log("You're too far away; get closer to the book");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Todo: Tương lai có thể hiện UI popup nhỏ báo hiệu "[E] Đọc sách"
    }
}