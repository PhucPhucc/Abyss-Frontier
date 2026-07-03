using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Torch : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Light2D light2D;

    private bool lit = false;
    private TorchPromptUI promptUI;

    private void Awake()
    {
        // TorchPromptUI có thể nằm trên cùng GameObject
        promptUI = GetComponent<TorchPromptUI>();
    }

    public void LightTorch()
    {
        if (lit) return;

        lit = true;

        animator.SetBool("Lit", true);

        light2D.intensity = 1.8f;
        light2D.enabled = true;

        // Ẩn prompt ngay khi đuốc được thắp
        if (promptUI != null)
            promptUI.SetVisible(false);
    }

    public bool IsLit()
    {
        return lit;
    }

    /// <summary>Yêu cầu hiện/ẩn prompt (bị bỏ qua nếu đuốc đã sáng).</summary>
    public void ShowPrompt(bool show)
    {
        if (promptUI == null) return;
        // Không hiện lại nếu đã thắp
        promptUI.SetVisible(show && !lit);
    }
}