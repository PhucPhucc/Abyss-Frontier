using UnityEngine;

public class SpumAnimatorDriver : CharacterAnimationHandler
{
    private SPUM_Prefabs spumPrefabs;
    private CharacterMotor motor;

    private void Awake()
    {
        spumPrefabs = GetComponent<SPUM_Prefabs>();
        motor = GetComponent<CharacterMotor>();
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

        Vector3 scale = transform.localScale;
        if (motor.LastDirection.x > 0)
            scale.x = -1;
        else if (motor.LastDirection.x < 0)
            scale.x = 1;
        transform.localScale = scale;

        PlayerState state = motor.IsMoving ? PlayerState.MOVE : PlayerState.IDLE;
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
}
