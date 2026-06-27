using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Boss Mimic tầng 4 — giả dạng rương đóng (IdleClosed), mở ra khi Player tương tác,
/// rồi transform và xen kẽ 2 đòn tấn công.
/// </summary>
public class MimicBossController : BossController
{
    [Header("Mimic Intro")]
    [SerializeField] private float interactRange = 1.6f;
    [SerializeField] private float openingAnimDuration = 0.8f;
    [SerializeField] private float transformAnimDuration = 0.7f;

    [Header("Mimic Attacks")]
    [SerializeField] private string secondAttackTrigger = "attack2";

    private bool useSecondAttack;
    private bool awakenRequested;
    private bool playerInInteractRange;

    protected override IEnumerator IntroRoutine()
    {
        state = BossState.Intro;
        rb.linearVelocity = Vector2.zero;

        if (health != null)
            health.CanTakeDamage = false;

        if (anim != null)
        {
            anim.SetBool("isMoving", false);
            anim.Play("IdleClosed", 0, 0f);
        }

        while (!awakenRequested && !IsDead)
        {
            CheckPlayerInteraction();
            yield return null;
        }

        if (IsDead)
            yield break;

        if (anim != null)
        {
            anim.Play("Opening", 0, 0f);
            yield return new WaitForSeconds(openingAnimDuration);
            anim.Play("Transform", 0, 0f);
            yield return new WaitForSeconds(transformAnimDuration);
            anim.Play("Idle", 0, 0f);
        }
        else
        {
            yield return new WaitForSeconds(openingAnimDuration + transformAnimDuration);
        }

        if (health != null)
            health.CanTakeDamage = true;

        state = BossState.Chase;
    }

    private void CheckPlayerInteraction()
    {
        if (target == null)
            return;

        if (!playerInInteractRange)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist > interactRange)
                return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            awakenRequested = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            playerInInteractRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            playerInInteractRange = false;
    }

    protected override string GetAttackTrigger()
    {
        if (useSecondAttack)
        {
            useSecondAttack = false;
            return secondAttackTrigger;
        }

        useSecondAttack = true;
        return base.GetAttackTrigger();
    }
}
