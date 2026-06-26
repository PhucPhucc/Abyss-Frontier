using UnityEngine;

public class HeroAnimatorDriver : CharacterAnimationHandler
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

        animator.SetFloat("moveX", motor.LastDirection.x);
        animator.SetFloat("moveY", motor.LastDirection.y);
        animator.SetFloat("lastMoveX", motor.LastDirection.x);
        animator.SetFloat("lastMoveY", motor.LastDirection.y);
        // animator.SetBool("isWalking", motor.IsMoving);
        animator.SetBool("isRunning", IsDashOrSprintActive());
    }

    private bool IsDashOrSprintActive()
    {
        if (playerDash != null && playerDash.IsDashing)
            return true;

        return playerController != null && playerController.IsSprinting;
    }

    public override void TriggerHurt()
    {
        // animator?.SetTrigger("hurt");
    }

    public override void TriggerAttack()
    {
        animator?.SetTrigger("attack");
    }

    public override void TriggerDeath()
    {
        // animator?.SetTrigger("death");
    }

    public override void TriggerRespawn()
    {
        if (animator != null)
            animator.speed = 1f;
    }
}
