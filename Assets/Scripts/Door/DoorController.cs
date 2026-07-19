using System.Collections;
using UnityEngine;
using Fusion;

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

    [Networked]
    private NetworkBool NetworkedOpen { get; set; }

    private ChangeDetector _changeDetector;
    private bool _isOpen = false;

    // ── Lifecycle ────────────────────────────────────────────────────────────────

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(NetworkedOpen) && NetworkedOpen)
                OpenDoorLocal();
        }
    }

    // ── API ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi từ InteractableLever. Trong multiplayer, chỉ gọi trên Host (StateAuthority).
    /// </summary>
    public void OpenDoor()
    {
        if (_isOpen) return;

        // Singleplayer hoặc không có Runner
        if (Runner == null || Runner.GameMode == GameMode.Single)
        {
            OpenDoorLocal();
            return;
        }

        // Multiplayer: Host set networked state, tự động broadcast qua Render()
        if (Object.HasStateAuthority)
        {
            NetworkedOpen = true;
        }
    }

    // ── Local Door Open ──────────────────────────────────────────────────────────

    private void OpenDoorLocal()
    {
        if (_isOpen) return;
        _isOpen = true;

        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");

        StartCoroutine(DisableColliderAfterAnimation());
    }

    private IEnumerator DisableColliderAfterAnimation()
    {
        AnimatorClipInfo[] clips = doorAnimator != null
            ? doorAnimator.GetCurrentAnimatorClipInfo(0)
            : System.Array.Empty<AnimatorClipInfo>();

        float clipLength = 0.8f;
        if (clips.Length > 0)
            clipLength = clips[0].clip.length;

        yield return new WaitForSeconds(clipLength);

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorTopRenderer != null && openTopSprite != null)
            doorTopRenderer.sprite = openTopSprite;

        if (doorBottomRenderer != null && openBottomSprite != null)
            doorBottomRenderer.sprite = openBottomSprite;
    }
}