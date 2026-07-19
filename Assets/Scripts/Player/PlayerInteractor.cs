using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public interface IInteractable
{
    void Interact(GameObject interactor);
    void ShowPrompt(bool show);
}

/// <summary>
/// Quản lý các tương tác của Player với các object trong môi trường.
/// Tự động gọi khi nhấn action "Interact" (Hold mặc định).
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    private List<IInteractable> interactables = new List<IInteractable>();

    public void RegisterInteractable(IInteractable interactable)
    {
        if (!interactables.Contains(interactable))
        {
            interactables.Add(interactable);
            Debug.Log($"[PlayerInteractor] Registered interactable: {interactable.GetType().Name}");
        }
    }

    public void UnregisterInteractable(IInteractable interactable)
    {
        interactables.Remove(interactable);
        Debug.Log($"[PlayerInteractor] Unregistered interactable: {interactable.GetType().Name}");
    }

    public void OnInteract(InputValue value)
    {
        Debug.Log($"[PlayerInteractor] OnInteract received. isPressed: {value.isPressed}, Count: {interactables.Count}");
        if (!value.isPressed) return;
        TriggerInteract();
    }

    /// <summary>
    /// Gọi trực tiếp từ code (ví dụ: NetworkPlayer trong Fusion tick) để kích hoạt interact.
    /// </summary>
    public void TriggerInteract()
    {
        // Dọn dẹp các object bị null (do đã bị destroy)
        interactables.RemoveAll(i => i == null || (i is Object obj && obj == null));

        if (interactables.Count > 0)
        {
            var target = interactables[interactables.Count - 1];
            Debug.Log($"[PlayerInteractor] Interacting with: {target.GetType().Name}");
            target.Interact(gameObject);
        }
    }
}