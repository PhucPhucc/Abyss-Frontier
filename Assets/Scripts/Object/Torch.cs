using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Torch : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Light2D light2D;

    private bool lit = false;

    public void LightTorch()
    {
        if (lit) return;

        lit = true;

        animator.SetBool("Lit", true);

        light2D.intensity = 1.8f;
        light2D.enabled = true;
    }

    public bool IsLit()
    {
        return lit;
    }
}