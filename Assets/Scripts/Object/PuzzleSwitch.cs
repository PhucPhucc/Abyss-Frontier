using UnityEngine;

public enum SwitchType { Sun, Moon, Fire, Earth, Wind, Water }

[RequireComponent(typeof(InteractableTrigger))]
public class PuzzleSwitch : MonoBehaviour, IInteractable
{
    public SwitchType myType;
    private bool isActivated = false;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Interact(GameObject interactor)
    {
        // Nếu đã gạt rồi thì bỏ qua
        if (isActivated) return;

        isActivated = true;
        if (anim != null)
        {
            Debug.Log("myType: " + myType);
            anim.SetBool("IsOn", true); // Kích hoạt hoạt ảnh gạt xuống
        }

        // Báo cho trọng tài biết nút này vừa được gạt
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnSwitchActivated(myType, this);
        }
    }

    public void ShowPrompt(bool show)
    {
        // Todo: Có thể gắn thêm UI hiển thị "[E] Gạt cần" ở đây giống như Torch
    }

    // Hàm này để Manager gọi khi người chơi giải sai và cần gạt nảy lên lại
    public void ResetSwitch()
    {
        isActivated = false;
        if (anim != null)
        {
            anim.SetBool("IsOn", false); // Kích hoạt hoạt ảnh nảy lên
        }
    }
}