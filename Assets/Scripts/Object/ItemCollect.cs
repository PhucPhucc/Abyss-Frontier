using Fusion;
using UnityEngine;

public class ItemCollect : NetworkBehaviour
{
    [Networked] private NetworkBool IsCollected { get; set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsCollected)
            return;

        // Kiểm tra xem đối tượng va chạm có tag là "Player" không
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Coin Collected!");

            if (Object.HasStateAuthority)
            {
                Collect();
            }
            else if (Object.HasInputAuthority || Runner != null)
            {
                RPC_RequestCollect();
            }
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
