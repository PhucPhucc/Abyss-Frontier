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
        }
    }

    public void UnregisterInteractable(IInteractable interactable)
    {
        interactables.Remove(interactable);
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        // Dọn dẹp các object bị null (do đã bị destroy)
        interactables.RemoveAll(i => i == null || (i is Object obj && obj == null));

        if (interactables.Count > 0)
        {
            // Tương tác với object gần nhất (vào trigger sau cùng)
            interactables[interactables.Count - 1].Interact(gameObject);
        }
    }
}