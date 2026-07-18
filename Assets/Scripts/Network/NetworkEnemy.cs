using System.Collections;
using Fusion;
using UnityEngine;

/// <summary>
/// Network wrapper cho Enemy. Server chạy AI, sync vị trí/state về clients qua Fusion.
/// Client chỉ render interpolated state từ NetworkTransform + [Networked] properties.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(NetworkObject))]
public class NetworkEnemy : NetworkBehaviour
{
    [Networked] public int SyncHealth { get; set; }
    [Networked] public int SyncMaxHealth { get; set; }
    [Networked] public NetworkBool SyncIsDead { get; set; }
    [Networked] public Vector2 SyncMoveVelocity { get; set; }
    [Networked] public Vector2 SyncLastDirection { get; set; }
    [Networked] public NetworkBool SyncIsMoving { get; set; }
    [Networked] public float SyncMoveX { get; set; }
    [Networked] public float SyncMoveY { get; set; }
    [Networked] public float SyncLastMoveX { get; set; }
    [Networked] public float SyncLastMoveY { get; set; }

    private EnemyHealth enemyHealth;
    private EnemyAI enemyAI;
    private BossController bossController;
    private KnockbackHandler knockbackHandler;
    private Animator anim;
    private bool hasMoveParams;
    private bool isNetworked;

    public bool IsServerAuth => Object.HasStateAuthority;

    public override void Spawned()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyAI = GetComponent<EnemyAI>();
        bossController = GetComponent<BossController>();
        knockbackHandler = GetComponent<KnockbackHandler>();
        anim = GetComponent<Animator>();

        isNetworked = Runner != null &&
            Runner.GameMode != GameMode.Single &&
            Runner.GameMode != GameMode.Shared;

        if (!isNetworked)
        {
            if (TryGetComponent<NetworkTransform>(out var nt))
                nt.enabled = false;
            return;
        }

        if (anim != null)
        {
            foreach (var p in anim.parameters)
            {
                if (p.name == "lastMoveX")
                {
                    hasMoveParams = true;
                    break;
                }
            }
        }

        if (!Object.HasStateAuthority)
        {
            if (enemyAI != null) enemyAI.enabled = false;
            if (bossController != null) bossController.enabled = false;
            if (knockbackHandler != null) knockbackHandler.enabled = false;

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!isNetworked) return;

        if (Object.HasStateAuthority)
            SyncToNetwork();
        else
            ApplyFromNetwork();
    }

    private void SyncToNetwork()
    {
        if (enemyHealth != null)
        {
            SyncHealth = enemyHealth.CurrentHealth;
            SyncMaxHealth = enemyHealth.MaxHealth;
            SyncIsDead = enemyHealth.IsDead;
        }

        if (enemyAI != null)
        {
            SyncMoveVelocity = enemyAI.MoveVelocity;
            SyncLastDirection = enemyAI.LastDirection;
            SyncIsMoving = enemyAI.IsMoving;
        }

        if (anim != null && hasMoveParams)
        {
            SyncMoveX = anim.GetFloat("moveX");
            SyncMoveY = anim.GetFloat("moveY");
            SyncLastMoveX = anim.GetFloat("lastMoveX");
            SyncLastMoveY = anim.GetFloat("lastMoveY");
        }
    }

    private void ApplyFromNetwork()
    {
        if (enemyHealth != null && enemyHealth.CurrentHealth != SyncHealth)
            enemyHealth.SetCurrentHealth(SyncHealth);

        if (anim != null)
        {
            if (hasMoveParams)
            {
                anim.SetFloat("moveX", SyncMoveX);
                anim.SetFloat("moveY", SyncMoveY);
                anim.SetFloat("lastMoveX", SyncLastMoveX);
                anim.SetFloat("lastMoveY", SyncLastMoveY);
            }
            anim.SetBool("isMoving", SyncIsMoving);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestDamage(int damage, Vector2 knockbackDir, float knockbackDuration = 0.15f, float stunDuration = -1f)
    {
        if (enemyHealth != null && !enemyHealth.IsDead)
            enemyHealth.TakeDamage(damage, knockbackDir, knockbackDuration, stunDuration);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayHurt()
    {
        if (!Object.HasStateAuthority)
        {
            if (anim != null) anim.SetTrigger("hurt");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_BroadcastDie()
    {
        if (anim != null) anim.SetTrigger("die");

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}
