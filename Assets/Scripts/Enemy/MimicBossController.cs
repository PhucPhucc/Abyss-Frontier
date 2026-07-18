using System.Collections;
using UnityEngine;

/// <summary>
/// Boss Mimic tầng 4 — intro transform (rương mở ra) và xen kẽ 2 đòn tấn công.
/// </summary>
public class MimicBossController : BossController
{
    [Header("Mimic Intro")]
    [SerializeField] private float openingAnimDuration = 0.8f;
    [SerializeField] private float transformAnimDuration = 0.7f;

    [Header("Mimic Attacks")]
    [SerializeField] private string secondAttackTrigger = "attack2";

    private bool useSecondAttack;

    protected override IEnumerator IntroRoutine()
    {
        state = BossState.Intro;
        rb.linearVelocity = Vector2.zero;
        if (anim != null)
        {
            anim.SetBool("isMoving", false);
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

        state = BossState.Idle;
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
