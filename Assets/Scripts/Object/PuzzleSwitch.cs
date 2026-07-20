using UnityEngine;
using Fusion;

public enum SwitchType { Sun, Moon, Fire, Earth, Wind, Water }

[RequireComponent(typeof(InteractableTrigger))]
public class PuzzleSwitch : NetworkBehaviour, IInteractable
{
    public SwitchType myType;

    [Networked] public bool IsActivated { get; set; }

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void Spawned()
    {
        if (IsActivated && anim != null)
            anim.SetBool("IsOn", true);
    }

    public void Interact(GameObject interactor)
    {
        if (IsActivated) return;

        if (Object.HasStateAuthority)
        {
            ActivateSwitch();
        }
        else
        {
            RPC_RequestActivate();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestActivate()
    {
        ActivateSwitch();
    }

    private void ActivateSwitch()
    {
        if (IsActivated) return;
        IsActivated = true;

        if (anim != null)
        {
            Debug.Log("myType: " + myType);
            anim.SetBool("IsOn", true);
        }

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnSwitchActivated(myType, this);
        }
    }

    public void ShowPrompt(bool show)
    {
    }

    public void ResetSwitch()
    {
        if (!Object.HasStateAuthority) return;
        IsActivated = false;
        RPC_PlayResetAnimation();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayResetAnimation()
    {
        if (anim != null)
            anim.SetBool("IsOn", false);
    }
}
