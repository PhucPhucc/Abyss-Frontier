using UnityEngine;

public enum SwitchType { Sun, Moon, Fire, Earth, Wind, Water }

public class PuzzleSwitch : MonoBehaviour
{
    public SwitchType myType;
    private bool isActivated = false;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Khi người chơi chạm vào và công tắc chưa bật
        if (other.CompareTag("Player") && !isActivated)
        {
            Interact();
        }
    }

    private void Interact()
    {
        isActivated = true;
        anim.SetBool("IsOn", true); // Kích hoạt hoạt ảnh gạt xuống

        // Báo cho trọng tài biết nút này vừa được gạt
        PuzzleManager.Instance.OnSwitchActivated(myType, this);
    }

    // Hàm này để Manager gọi khi người chơi giải sai và cần gạt nảy lên lại
    public void ResetSwitch()
    {
        isActivated = false;
        anim.SetBool("IsOn", false); // Kích hoạt hoạt ảnh nảy lên
    }
}