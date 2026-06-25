using UnityEngine;

public abstract class CharacterAnimationHandler : MonoBehaviour
{
    public abstract void TriggerHurt();
    public abstract void TriggerAttack();
    public abstract void TriggerDeath();
}
