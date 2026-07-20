using System.Collections;
using Fusion;
using UnityEngine;

public class DoorController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Collider2D doorCollider;

    [Header("Door Parts")]
    [SerializeField] private SpriteRenderer doorTopRenderer;
    [SerializeField] private SpriteRenderer doorBottomRenderer;

    [Header("Open Sprites")]
    [SerializeField] private Sprite openTopSprite;
    [SerializeField] private Sprite openBottomSprite;

    [Networked] public bool IsOpen { get; set; }

    public override void Spawned()
    {
        if (IsOpen)
            ApplyOpenState();
    }

    public void OpenDoor()
    {
        if (IsOpen) return;

        if (Object.HasStateAuthority)
        {
            IsOpen = true;
            RPC_PlayOpenAnimation();
        }
        else
        {
            RPC_RequestOpen();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestOpen()
    {
        if (!IsOpen)
        {
            IsOpen = true;
            RPC_PlayOpenAnimation();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayOpenAnimation()
    {
        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");

        StartCoroutine(DisableColliderAfterAnimation());
    }

    private IEnumerator DisableColliderAfterAnimation()
    {
        float clipLength = 0.8f;

        if (doorAnimator != null)
        {
            AnimatorClipInfo[] clips = doorAnimator.GetCurrentAnimatorClipInfo(0);
            if (clips.Length > 0)
                clipLength = clips[0].clip.length;
        }

        yield return new WaitForSeconds(clipLength);

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorTopRenderer != null)
            doorTopRenderer.sprite = openTopSprite;
        if (doorBottomRenderer != null)
            doorBottomRenderer.sprite = openBottomSprite;
    }

    private void ApplyOpenState()
    {
        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorTopRenderer != null)
            doorTopRenderer.sprite = openTopSprite;
        if (doorBottomRenderer != null)
            doorBottomRenderer.sprite = openBottomSprite;
    }
}
