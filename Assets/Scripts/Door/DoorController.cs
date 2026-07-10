using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
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

    private bool _isOpen = false;

    public void OpenDoor()
    {
        if (_isOpen) return;
        _isOpen = true;
        doorAnimator.SetTrigger("Open");
        StartCoroutine(DisableColliderAfterAnimation());
    }

    private IEnumerator DisableColliderAfterAnimation()
    {
        AnimatorClipInfo[] clips = doorAnimator.GetCurrentAnimatorClipInfo(0);
        float clipLength = 0.8f;

        if (clips.Length > 0)
            clipLength = clips[0].clip.length;

        yield return new WaitForSeconds(clipLength);

        doorCollider.enabled = false;

        doorTopRenderer.sprite = openTopSprite;
        doorBottomRenderer.sprite = openBottomSprite;
    }
}