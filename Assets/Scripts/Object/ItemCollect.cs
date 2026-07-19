using Fusion;
using UnityEngine;

public class ItemCollect : NetworkBehaviour
{
    [Networked] private NetworkBool IsCollected { get; set; }

    // Fallback cho trường hợp object không được spawn qua Fusion (scene object / single player)
    private bool _localCollected = false;

    private bool IsFusionReady => Object != null && Object.IsValid;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        // Nếu Fusion chưa ready (scene object / offline mode), dùng local flag
        if (!IsFusionReady)
        {
            if (_localCollected) return;
            _localCollected = true;
            Debug.Log("Key Collected! (local mode)");
            Destroy(gameObject);
            return;
        }

        if (IsCollected)
            return;

        Debug.Log("Key Collected! (networked mode)");

        if (Object.HasStateAuthority)
        {
            Collect();
        }
        else
        {
            RPC_RequestCollect();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestCollect()
    {
        Collect();
    }

    private void Collect()
    {
        if (IsCollected)
            return;

        IsCollected = true;

        if (Runner != null && Object.HasStateAuthority)
            Runner.Despawn(Object);
        else
            Destroy(gameObject);
    }
}
