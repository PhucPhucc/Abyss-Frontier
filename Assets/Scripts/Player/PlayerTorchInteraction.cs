using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Nhấn F (hoặc E) khi đứng trong vùng trigger của đuốc để thắp sáng.
/// Yêu cầu: PlayerInput component trên cùng GameObject.
/// </summary>
public class PlayerTorchInteraction : MonoBehaviour
{
    private Torch currentTorch;

    // Được gọi bởi TorchTrigger khi player bước vào vùng đuốc
    public void SetCurrentTorch(Torch torch)
    {
        currentTorch = torch;
    }

    // Được gọi bởi TorchTrigger khi player bước ra khỏi vùng đuốc
    public void ClearTorch(Torch torch)
    {
        if (currentTorch == torch)
            currentTorch = null;
    }

    // Được gọi tự động bởi PlayerInput khi nhấn action "Interact" (phím F hoặc E)
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;
        if (currentTorch == null) return;
        if (currentTorch.IsLit()) return;

        currentTorch.LightTorch();
    }
}