using UnityEngine;

public class TorchTrigger : MonoBehaviour
{
    public Torch torch;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerTorchInteraction interaction =
                other.GetComponent<PlayerTorchInteraction>();

            interaction.SetCurrentTorch(torch);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerTorchInteraction interaction =
                other.GetComponent<PlayerTorchInteraction>();

            interaction.ClearTorch(torch);
        }
    }
}