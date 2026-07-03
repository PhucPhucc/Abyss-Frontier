using System.Collections;
using UnityEngine;

public class StandardAnimatorDriver : CharacterAnimationHandler
{
    private Animator animator;
    private CharacterMotor motor;
    private PlayerStats playerStats;
    private PlayerController playerController;
    private PlayerDash playerDash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        motor = GetComponent<CharacterMotor>();
        playerStats = GetComponent<PlayerStats>();
        playerController = GetComponent<PlayerController>();
        playerDash = GetComponent<PlayerDash>();
    }

    private void Update()
    {
        if (animator == null || motor == null)
            return;

        if (playerStats != null && playerStats.IsDead)
            return;

        // Smooth moveX/moveY để tránh animation giật khi MoveInput đến từ
        // Fusion tick rate thay vì mọi frame (trường hợp spawn từ menu)
        float targetX = motor.LastDirection.x;
        float targetY = motor.LastDirection.y;
        float smoothSpeed = Time.deltaTime * 15f;
        float currentX = animator.GetFloat("moveX");
        float currentY = animator.GetFloat("moveY");
        animator.SetFloat("moveX", Mathf.MoveTowards(currentX, targetX, smoothSpeed));
        animator.SetFloat("moveY", Mathf.MoveTowards(currentY, targetY, smoothSpeed));
        animator.SetBool("isWalk", motor.IsMoving || (playerDash != null && playerDash.IsDashing));
        animator.SetBool("isRun", IsDashOrSprintActive());
    }

    private bool IsDashOrSprintActive()
    {
        if (playerDash != null && playerDash.IsDashing)
            return true;

        return playerController != null && playerController.IsSprinting;
    }

    public override void TriggerHurt()
    {
        SetTriggerIfExists("hurt");
    }

    public override void TriggerAttack()
    {
        SetTriggerIfExists("Attack");
        SetTriggerIfExists("attack");
    }

    public override void TriggerDeath()
    {
        SetTriggerIfExists("die");
        SetTriggerIfExists("death");
    }

    public override IEnumerator WaitForDeathAnimationRoutine()
    {
        if (animator == null)
        {
            yield return new WaitForSeconds(1f);
            yield break;
        }

        const float enterTimeout = 0.5f;
        float enterElapsed = 0f;
        while (enterElapsed < enterTimeout && !IsInDeathState())
        {
            enterElapsed += Time.deltaTime;
            yield return null;
        }

        while (IsInDeathState() && GetDeathNormalizedTime() < 1f)
            yield return null;

        if (animator != null)
            animator.speed = 0f;
    }

    private bool IsInDeathState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Death") || stateInfo.IsName("die");
    }

    private float GetDeathNormalizedTime()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime;
    }

    public override void TriggerRespawn()
    {
        if (animator == null)
            return;

        animator.speed = 1f;
        ResetTriggerIfExists("die");
        ResetTriggerIfExists("death");
        ResetTriggerIfExists("hurt");
        ResetTriggerIfExists("Attack");
        ResetTriggerIfExists("attack");
    }

    private void SetTriggerIfExists(string parameterName)
    {
        if (animator == null || !HasParameter(parameterName))
            return;

        animator.SetTrigger(parameterName);
    }

    private void ResetTriggerIfExists(string parameterName)
    {
        if (animator == null || !HasParameter(parameterName))
            return;

        animator.ResetTrigger(parameterName);
    }

    private bool HasParameter(string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
                return true;
        }

        return false;
    }
}
