using UnityEngine;

public class HeroAnimatorDriver : CharacterAnimationHandler
{
    private Animator animator;
    private CharacterMotor motor;
    private PlayerStats playerStats;
    private PlayerController playerController;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        motor = GetComponent<CharacterMotor>();
        playerStats = GetComponent<PlayerStats>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (animator == null || motor == null)
            return;

        if (playerStats != null && playerStats.IsDead)
        {
            // animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            return;
        }

        animator.SetFloat("moveX", motor.LastDirection.x);
        animator.SetFloat("moveY", motor.LastDirection.y);
        animator.SetFloat("lastMoveX", motor.LastDirection.x);
        animator.SetFloat("lastMoveY", motor.LastDirection.y);
        // animator.SetBool("isWalking", motor.IsMoving);
        animator.SetBool("isRunning", playerController != null && playerController.IsSprinting);
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
}
