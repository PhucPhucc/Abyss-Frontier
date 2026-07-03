using UnityEngine;

public class TorchTrigger : MonoBehaviour
{
    public Torch torch;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hiện prompt khi player bước vào vùng đuốc
            if (torch != null)
                torch.ShowPrompt(true);

            PlayerTorchInteraction interaction =
                other.GetComponent<PlayerTorchInteraction>();

            if (interaction != null)
                interaction.SetCurrentTorch(torch);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Ẩn prompt khi player bước ra
            if (torch != null)
                torch.ShowPrompt(false);

            PlayerTorchInteraction interaction =
                other.GetComponent<PlayerTorchInteraction>();

            if (interaction != null)
                interaction.ClearTorch(torch);
        }
    }
}