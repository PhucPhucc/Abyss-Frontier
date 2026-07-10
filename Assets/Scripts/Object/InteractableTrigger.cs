using UnityEngine;

/// <summary>
/// Phát hiện Player bước vào vùng tương tác và đăng ký với PlayerInteractor.
/// Gắn script này vào Trigger Collider 2D của object tương tác.
/// </summary>
public class InteractableTrigger : MonoBehaviour
{
    private IInteractable interactable;

    private void Awake()
    {
        // Thử tìm IInteractable trên cùng GameObject hoặc Component cha
        interactable = GetComponent<IInteractable>();
        if (interactable == null)
            interactable = GetComponentInParent<IInteractable>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactable != null)
                interactable.ShowPrompt(true);

            PlayerInteractor interactor = other.GetComponent<PlayerInteractor>();
            if (interactor != null && interactable != null)
            {
                interactor.RegisterInteractable(interactable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactable != null)
                interactable.ShowPrompt(false);

            PlayerInteractor interactor = other.GetComponent<PlayerInteractor>();
            if (interactor != null && interactable != null)
            {
                interactor.UnregisterInteractable(interactable);
            }
        }
    }
}