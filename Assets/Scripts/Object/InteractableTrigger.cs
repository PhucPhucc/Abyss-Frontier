using UnityEngine;

/// <summary>
/// Phát hiện Player bước vào vùng tương tác và đăng ký với PlayerInteractor.
/// Gắn script này vào Trigger Collider 2D của object tương tác.
/// </summary>
public class InteractableTrigger : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Drag and drop this object's own Text Object or Text Panel (e.g., Press E) here.")]
    public GameObject promptUI;
    private IInteractable interactable;

    private void Awake()
    {
        // Thử tìm IInteractable trên cùng GameObject hoặc Component cha
        interactable = GetComponent<IInteractable>();
        if (interactable == null)
            interactable = GetComponentInParent<IInteractable>();

        // Đảm bảo có Collider 2D làm Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 1.5f); // Vùng tương tác 1.5x1.5
            Debug.Log($"[InteractableTrigger] Automatically added BoxCollider2D (Is Trigger) to {gameObject.name}");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.Log($"[InteractableTrigger] Set existing Collider2D as Trigger on {gameObject.name}");
        }
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[InteractableTrigger] OnTriggerEnter2D with: {other.name}, Tag: {other.tag}");
        if (other.CompareTag("Player"))
        {
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }

            if (interactable != null)
                interactable.ShowPrompt(true);

            PlayerInteractor interactor = other.GetComponent<PlayerInteractor>();
            if (interactor != null && interactable != null)
            {
                interactor.RegisterInteractable(interactable);
                Debug.Log($"[InteractableTrigger] Successfully registered {gameObject.name} with PlayerInteractor on {other.name}");
            }
            else
            {
                Debug.LogWarning($"[InteractableTrigger] Failed to register: interactor is null? {interactor == null}, interactable is null? {interactable == null}");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[InteractableTrigger] OnTriggerExit2D with: {other.name}");
        if (other.CompareTag("Player"))
        {
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
                
            if (interactable != null)
                interactable.ShowPrompt(false);

            PlayerInteractor interactor = other.GetComponent<PlayerInteractor>();
            if (interactor != null && interactable != null)
            {
                interactor.UnregisterInteractable(interactable);
                Debug.Log($"[InteractableTrigger] Successfully unregistered {gameObject.name} from PlayerInteractor on {other.name}");
            }
        }
    }
}