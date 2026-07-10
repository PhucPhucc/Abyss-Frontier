using UnityEngine;
using UnityEngine.InputSystem;

public enum SwitchType { Sun, Moon, Fire, Earth, Wind, Water }

[RequireComponent(typeof(InteractableTrigger))]
public class PuzzleSwitch : MonoBehaviour, IInteractable
{
    public SwitchType myType;
    private Animator anim;

    [Header("Interaction Settings")]
    [Tooltip("Khoảng cách tối đa để nhân vật có thể gạt công tắc này")]
    public float interactionRange = 1.5f;

    private bool isAlreadyOn = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // 1. Kiểm tra sự kiện ấn phím O (Hệ thống Input System mới)
        if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // Nếu công tắc này đã gạt rồi thì bỏ qua
        if (isAlreadyOn) return;

        // 2. Tìm nhân vật bằng Tag theo đúng dòng code bạn cung cấp
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        // Nếu không tìm thấy nhân vật nào có Tag là "Player" trên Map thì dừng lại
        if (playerObj == null)
        {
            Debug.LogWarning("Không tìm thấy Object nào có Tag là 'Player'!");
            return;
        }

        // 3. Tính khoảng cách toán học giữa công tắc này và nhân vật
        float distance = Vector2.Distance(transform.position, playerObj.transform.position);

        // 4. Chỉ công tắc nào ở sát nhân vật (nhỏ hơn khoảng cách cho phép) thì mới gạt
        if (distance <= interactionRange)
        {
            Interact();
        }
    }

    private void Interact()
    {
        isAlreadyOn = true;
        anim.SetBool("IsOn", true); // Chạy hoạt ảnh gạt công tắc xuống

        // --- ĐÃ THÊM DEBUG LOG Ở ĐÂY ---
        Debug.Log("Player vừa gạt công tắc loại: " + myType.ToString());
        // -------------------------------

        // Báo cho PuzzleManager (trọng tài giải đố) biết
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnSwitchActivated(myType, this);
        }
    }

    // Hàm này để PuzzleManager gọi reset khi người chơi giải sai câu đố
    public void ResetSwitch()
    {
        isAlreadyOn = false;
        anim.SetBool("IsOn", false); // Chạy hoạt ảnh công tắc nảy lên lại
    }

    // Vẽ vòng tròn màu xanh trong cửa sổ Scene để bạn dễ căn chỉnh độ dài interactionRange
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}