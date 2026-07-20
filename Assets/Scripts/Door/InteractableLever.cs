using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Fusion;

[RequireComponent(typeof(InteractableTrigger))]
public class InteractableLever : NetworkBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private DoorController linkedDoor;
    [SerializeField] private Animator leverAnimator;

    [Networked] public bool IsActivated { get; set; }

    private InteractPromptUI promptUI;

    private void Awake()
    {
        promptUI = GetComponent<InteractPromptUI>();
    }

    public void Interact(GameObject interactor)
    {
        if (IsActivated) return;

        if (Object.HasStateAuthority)
        {
            IsActivated = true;
        }
        else
        {
            RPC_RequestActivate();
        }

        if (promptUI != null)
            promptUI.SetVisible(false);

        StartCoroutine(CutsceneRoutine(interactor));
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestActivate()
    {
        IsActivated = true;
    }

    private IEnumerator CutsceneRoutine(GameObject player)
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.DeactivateInput();

        PlayerHealth pHealth = player.GetComponent<PlayerHealth>();
        if (pHealth != null) pHealth.SetInvulnerable(true);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Animator playerAnim = player.GetComponentInChildren<Animator>();
        if (playerAnim != null) playerAnim.SetBool("isWalk", false);

        EnemyAI[] enemies = UnityEngine.Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.enabled = false;
                enemy.StopAllCoroutines();

                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null) enemyRb.linearVelocity = Vector2.zero;

                Animator eAnim = enemy.GetComponentInChildren<Animator>();
                if (eAnim != null) eAnim.speed = 0f;
            }
        }

        leverAnimator?.SetTrigger("Pull");

        CinemachineCamera cam = UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>();
        Transform originalTarget = null;

        if (cam != null && linkedDoor != null)
        {
            originalTarget = cam.Target.TrackingTarget;
            cam.Target.TrackingTarget = linkedDoor.transform;
            yield return new WaitForSeconds(1f);
        }

        if (linkedDoor != null)
        {
            linkedDoor.OpenDoor();
            yield return new WaitForSeconds(1.0f);
        }

        if (cam != null && originalTarget != null)
        {
            cam.Target.TrackingTarget = originalTarget;
            yield return new WaitForSeconds(1f);
        }

        if (playerInput != null) playerInput.ActivateInput();
        if (pHealth != null) pHealth.SetInvulnerable(false);

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.enabled = true;
                Animator eAnim = enemy.GetComponentInChildren<Animator>();
                if (eAnim != null) eAnim.speed = 1f;
            }
        }
    }

    public void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetVisible(show && !IsActivated);
    }
}
