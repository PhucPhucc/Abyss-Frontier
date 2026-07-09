using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Torch : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private Light2D light2D;

    private bool lit = false;
    private InteractPromptUI promptUI;

    private void Awake()
    {
        promptUI = GetComponent<InteractPromptUI>();
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

    public void Interact(GameObject interactor)
    {
        if (!lit)
        {
            LightTorch();
        }
    }

    /// <summary>Yêu cầu hiện/ẩn prompt (bị bỏ qua nếu đuốc đã sáng).</summary>
    public void ShowPrompt(bool show)
    {
        if (promptUI == null) return;
        // Không hiện lại nếu đã thắp
        promptUI.SetVisible(show && !lit);
    }
}