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

    // Networked state: tất cả clients đều biết lever đã bị gạt chưa
    [Networked] private NetworkBool NetworkedActivated { get; set; }

    private ChangeDetector _changeDetector;
    private bool _localActivated = false;
    private InteractPromptUI promptUI;

    private void Awake()
    {
        promptUI = GetComponent<InteractPromptUI>();
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // Late-join: nếu lever đã được gạt trước khi client join, apply ngay
        if (NetworkedActivated && !_localActivated)
            ActivateLeverLocal(FindLocalPlayerObject());
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(NetworkedActivated) && NetworkedActivated && !_localActivated)
                ActivateLeverLocal(FindLocalPlayerObject());
        }
    }

    public void Interact(GameObject interactor)
    {
        if (_localActivated) return;

        // Singleplayer: Runner null hoặc Single mode → xử lý local
        if (Runner == null || Runner.GameMode == GameMode.Single)
        {
            ActivateLeverLocal(interactor);
            return;
        }

        // Multiplayer: bất kỳ client nào cũng có thể gửi RPC lên Host.
        // Scene objects không có InputAuthority → phải dùng RpcSources.All.
        if (Object.HasStateAuthority)
        {
            // Host tự xử lý trực tiếp
            if (NetworkedActivated) return;
            NetworkedActivated = true;
        }
        else
        {
            // Client gửi lên Host
            RPC_RequestActivate();
        }
    }

    // ── RPCs ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Bất kỳ client nào gửi lên Host yêu cầu kích hoạt lever.
    /// Dùng RpcSources.All vì scene objects không có InputAuthority.
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestActivate()
    {
        if (NetworkedActivated) return;
        NetworkedActivated = true;
        // ChangeDetector trong Render() sẽ tự broadcast state cho tất cả clients
    }

    // ── Local Activation ────────────────────────────────────────────────────────

    private void ActivateLeverLocal(GameObject player)
    {
        if (_localActivated) return;
        _localActivated = true;

        if (promptUI != null)
            promptUI.SetVisible(false);

        StartCoroutine(CutsceneRoutine(player));
    }

    private IEnumerator CutsceneRoutine(GameObject player)
    {
        // 1. Khóa input — hỗ trợ cả Singleplayer (PlayerInput) và Multiplayer (flag)
        PlayerInput playerInput = player != null ? player.GetComponent<PlayerInput>() : null;
        PlayerController playerController = player != null ? player.GetComponent<PlayerController>() : null;

        // Singleplayer: dùng PlayerInput.DeactivateInput()
        if (playerInput != null && playerInput.enabled)
            playerInput.DeactivateInput();

        // Multiplayer: lock movement qua PlayerController
        bool wasLocked = false;
        if (playerController != null)
        {
            wasLocked = playerController.InputLocked;
            playerController.InputLocked = true;
        }

        // BẬT BẤT TỬ cho Player
        PlayerHealth pHealth = player != null ? player.GetComponent<PlayerHealth>() : null;
        if (pHealth != null) pHealth.SetInvulnerable(true);

        Rigidbody2D rb = player != null ? player.GetComponent<Rigidbody2D>() : null;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Animator playerAnim = player != null ? player.GetComponentInChildren<Animator>() : null;
        if (playerAnim != null) playerAnim.SetBool("isWalk", false);

        // 2. ĐÓNG BĂNG QUÁI VẬT (Tương tự Pause)
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

        // 3. Gạt cần và Lia Camera
        leverAnimator?.SetTrigger("Pull");

        CinemachineCamera cam = UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>();
        Transform originalTarget = null;

        if (cam != null && linkedDoor != null)
        {
            originalTarget = cam.Target.TrackingTarget;
            cam.Target.TrackingTarget = linkedDoor.transform;

            yield return new WaitForSeconds(1f);
        }

        // 4. Mở cửa và chờ animation
        if (linkedDoor != null)
        {
            linkedDoor.OpenDoor();
            yield return new WaitForSeconds(1.0f);
        }

        // 5. Lia camera trở lại người chơi
        if (cam != null && originalTarget != null)
        {
            cam.Target.TrackingTarget = originalTarget;
            yield return new WaitForSeconds(1f);
        }

        // 6. PHỤC HỒI LẠI TRẠNG THÁI GAME
        if (playerInput != null && playerInput.enabled)
            playerInput.ActivateInput();

        if (playerController != null)
            playerController.InputLocked = wasLocked;

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
            promptUI.SetVisible(show && !_localActivated);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static GameObject FindLocalPlayerObject()
    {
        // Ưu tiên tìm NetworkPlayer có HasInputAuthority (multiplayer)
        NetworkPlayer[] all = UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        foreach (var np in all)
        {
            if (np.Object != null && np.Object.HasInputAuthority)
                return np.gameObject;
        }

        // Fallback singleplayer: tìm PlayerController đầu tiên
        PlayerController pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        return pc != null ? pc.gameObject : null;
    }
}