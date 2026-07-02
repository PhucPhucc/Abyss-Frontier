using UnityEngine;
using System.Collections;

public class PlayerTorchInteraction : MonoBehaviour
{
    private Torch currentTorch;

    private bool lighting = false;

    public void SetCurrentTorch(Torch torch)
    {
        currentTorch = torch;

        if (!lighting)
            StartCoroutine(LightRoutine());
    }

    public void ClearTorch(Torch torch)
    {
        if (currentTorch == torch)
            currentTorch = null;
    }

    IEnumerator LightRoutine()
    {
        lighting = true;

        Vector3 startPos = transform.position;

        float timer = 0;

        while (timer < 0.5f)
        {
            if (Vector3.Distance(startPos, transform.position) > 0.05f)
            {
                lighting = false;
                yield break;
            }

            timer += Time.deltaTime;

            yield return null;
        }

        if (currentTorch != null)
            currentTorch.LightTorch();

        lighting = false;
    }
}