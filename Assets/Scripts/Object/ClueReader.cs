using UnityEngine;

[RequireComponent(typeof(InteractableTrigger))]
public class ClueReader : MonoBehaviour, IInteractable
{
    [Header("UI Reference")]
    public GameObject clueUIPanel;

    // Không cần interactionRange và player reference nữa vì InteractableTrigger (Collider2D) sẽ lo việc quét khoảng cách

    public void Interact(GameObject interactor)
    {
        if (clueUIPanel != null)
        {
            // Bật/tắt panel khi người chơi bấm nút tương tác
            clueUIPanel.SetActive(!clueUIPanel.activeSelf);
        }
    }

    public void ShowPrompt(bool show)
    {
        // Todo: Tương lai có thể hiện UI popup nhỏ báo hiệu "[E] Đọc sách"
    }
}