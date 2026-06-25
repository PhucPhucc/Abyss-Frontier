using UnityEngine;

public class StandardAnimatorDriver : CharacterAnimationHandler
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
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);
            return;
        }

        animator.SetFloat("moveX", motor.LastDirection.x);
        animator.SetFloat("moveY", motor.LastDirection.y);
        animator.SetBool("isWalk", motor.IsMoving);
        animator.SetBool("isRun", playerController != null && playerController.IsSprinting);
    }

    public override void TriggerHurt()
    {
        animator?.SetTrigger("hurt");
    }

    public override void TriggerAttack()
    {
        animator?.SetTrigger("Attack");
    }

    public override void TriggerDeath()
    {
        animator?.SetTrigger("death");
    }
}
