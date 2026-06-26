using System.Collections;
using UnityEngine;

public class SpumAnimatorDriver : CharacterAnimationHandler
{
    private SPUM_Prefabs spumPrefabs;
    private CharacterMotor motor;
    private PlayerDash playerDash;
    private PlayerStats playerStats;

    private void Awake()
    {
        spumPrefabs = GetComponent<SPUM_Prefabs>();
        motor = GetComponent<CharacterMotor>();
        playerDash = GetComponent<PlayerDash>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        if (spumPrefabs != null)
            spumPrefabs.OverrideControllerInit();
    }

    private void Update()
    {
        if (spumPrefabs == null || motor == null)
            return;

        if (playerStats != null && playerStats.IsDead)
            return;

        Vector3 scale = transform.localScale;
        if (motor.LastDirection.x > 0)
            scale.x = -1;
        else if (motor.LastDirection.x < 0)
            scale.x = 1;
        transform.localScale = scale;

        PlayerState state = motor.IsMoving || (playerDash != null && playerDash.IsDashing)
            ? PlayerState.MOVE
            : PlayerState.IDLE;
        spumPrefabs.PlayAnimation(state, 0);
    }

    public override void TriggerHurt()
    {
        spumPrefabs?.PlayAnimation(PlayerState.DAMAGED, 0);
    }

    public override void TriggerAttack()
    {
        spumPrefabs?.PlayAnimation(PlayerState.ATTACK, 0);
    }

    public override void TriggerDeath()
    {
        spumPrefabs?.PlayAnimation(PlayerState.DEATH, 0);
    }

    public override IEnumerator WaitForDeathAnimationRoutine()
    {
        if (spumPrefabs == null || spumPrefabs._anim == null)
        {
            yield return new WaitForSeconds(1f);
            yield break;
        }

        float duration = 1f;
        if (spumPrefabs.DEATH_List.Count > 0 && spumPrefabs.DEATH_List[0] != null)
            duration = spumPrefabs.DEATH_List[0].length;

        Animator animator = spumPrefabs._anim;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1f)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        animator.speed = 0f;
    }

    public override void TriggerRespawn()
    {
        if (spumPrefabs?._anim != null)
            spumPrefabs._anim.speed = 1f;

        spumPrefabs?.PlayAnimation(PlayerState.IDLE, 0);
    }
}
