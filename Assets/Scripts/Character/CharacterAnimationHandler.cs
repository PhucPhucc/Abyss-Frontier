using System.Collections;
using UnityEngine;

public abstract class CharacterAnimationHandler : MonoBehaviour
{
    public abstract void TriggerHurt();
    public abstract void TriggerAttack();
    public abstract void TriggerDeath();
    public virtual void TriggerRespawn() { }

    public virtual IEnumerator WaitForDeathAnimationRoutine()
    {
        yield return new WaitForSeconds(1f);
    }
}
