using Fusion;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerDash))]
public class NetworkPlayer : NetworkBehaviour
{
    private PlayerController playerController;
    private PlayerCombat playerCombat;
    private PlayerStats playerStats;
    private PlayerHealth playerHealth;
    private PlayerDash playerDash;
    private PlayerInput playerInput;
    private Rigidbody2D rb;

    private float attackCooldownTimer;
    private float attackHitPendingTimer;
    private bool isMultiplayer;

    [Networked] private Vector2 NetworkedMoveInput { get; set; }
    [Networked] private Vector2 NetworkedLastDirection { get; set; }
    [Networked] private NetworkBool NetworkedSprintPressed { get; set; }

    /// <summary>
    /// Gọi bởi EnemyAI trên Server để gửi damage tới đúng client sở hữu player.
    /// Trong multiplayer, EnemyAI chạy trên Host/Server — nó không thể gọi
    /// playerStats.TakeDamage() trực tiếp vì PlayerHealth chỉ tồn tại đầy đủ trên client owner.
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_TakeDamage(int damage)
    {
        playerHealth?.TakeDamage(damage);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_RequestRespawn(Vector2 respawnPosition)
    {
        if (rb != null)
        {
            rb.position = respawnPosition;
            rb.linearVelocity = Vector2.zero;
        }

        playerHealth?.Respawn();
    }

    private void Awake()
    {
        TryGetComponent(out playerController);
        TryGetComponent(out playerCombat);
        TryGetComponent(out playerStats);
        TryGetComponent(out playerHealth);
        TryGetComponent(out playerDash);
        TryGetComponent(out playerInput);
        TryGetComponent(out rb);
    }

    public override void Spawned()
    {
        // Trong GameMode.Single (singleplayer), giữ PlayerInput chạy bình thường
        // Để OnMove(InputValue) hoạt động → smooth movement trực tiếp
        // Chỉ disable PlayerInput trong multiplayer thực (Host/Client) để
        // tránh conflict với Fusion network input
        isMultiplayer = Runner != null &&
            Runner.GameMode != GameMode.Single &&
            Runner.GameMode != GameMode.Shared;

        if (playerInput != null)
            playerInput.enabled = !isMultiplayer;

        if (playerController != null)
            playerController.IsControlledByNetwork = isMultiplayer && Object.HasStateAuthority && !Object.HasInputAuthority;

        if (playerCombat != null)
            playerCombat.UseNetworkInput = isMultiplayer;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }

        if (Object.HasInputAuthority)
            AssignCameraTarget();
    }

    private const float PlayerOrthoSize = 3f;

    private void AssignCameraTarget()
    {
        var cam = FindFirstObjectByType<CinemachineCamera>();
        if (cam == null)
        {
            GameObject camObj = new GameObject("PlayerCamera", typeof(CinemachineCamera));
            cam = camObj.GetComponent<CinemachineCamera>();

            cam.transform.SetPositionAndRotation(
                new Vector3(transform.position.x, transform.position.y, -10f),
                Quaternion.identity);

            var follow = camObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0, 0, -10);

            var brain = FindFirstObjectByType<CinemachineBrain>();
            if (brain == null)
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                    mainCam.gameObject.AddComponent<CinemachineBrain>();
            }
        }

        var lens = cam.Lens;
        lens.OrthographicSize = PlayerOrthoSize;
        cam.Lens = lens;
        cam.Follow = transform;
    }

    public override void FixedUpdateNetwork()
    {
        // Trong Singleplayer, PlayerInput và Update/FixedUpdate cục bộ tự xử lý trọn vẹn di chuyển & tấn công.
        // Không cho phép FixedUpdateNetwork ghi đè MoveInput/Sprint từ network buffer.
        if (Runner != null && (Runner.GameMode == GameMode.Single || Runner.GameMode == GameMode.Shared))
            return;

        if (GetInput<NetworkInputData>(out var input) == false)
            return;

        if (playerController != null)
        {
            playerController.MoveInput = input.movement;
            playerController.SetSprintInput(input.IsSprintSet);

            // Chỉ state authority của player remote mới cần mô phỏng physics ở Fusion tick.
            // Local player chạy physics ở Unity FixedUpdate để mượt hơn.
            if (Object.HasStateAuthority && !Object.HasInputAuthority && !playerController.IsDashing)
                playerController.ApplyNetworkVelocity();
        }

        if (input.IsDodgeSet && playerDash != null)
            playerDash.TryDash();

        if (Object.HasStateAuthority && playerController != null)
        {
            NetworkedMoveInput = playerController.MoveInput;
            NetworkedSprintPressed = playerController.IsSprinting;
            NetworkedLastDirection = playerController.LastDirection;
        }

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Runner.DeltaTime;

        // Timer chứ sau đó thực hiện sát thương — chạy trên InputAuthority (client tự hiểu thị).
        // Client gọi TriggerAttackDamage() → tìm NetworkEnemy trong tầm → gọi RPC_RequestDamage lên Server.
        if (attackHitPendingTimer > 0f && Object.HasInputAuthority)
        {
            attackHitPendingTimer -= Runner.DeltaTime;
            if (attackHitPendingTimer <= 0f && playerCombat != null)
            {
                Debug.Log($"[NetworkPlayer] TriggerAttackDamage trên InputAuthority");
                playerCombat.TriggerAttackDamage(isHostPlayer: Object.HasStateAuthority);
            }
        }

        // Attack được trigger bởi InputAuthority — client tự chạy animation và đết cooldown.
        // Sau đó broadcast animation qua RPC cho các client khác thấy.
        if (input.IsAttackSet && attackCooldownTimer <= 0f && Object.HasInputAuthority)
        {
            attackCooldownTimer = 0.5f;
            attackHitPendingTimer = 0.2f;
            if (playerCombat != null)
                playerCombat.SetAttackCooldown(0.5f);

            Debug.Log($"[NetworkPlayer] Attack triggered trên InputAuthority");

            // Animation local (thực hiện ngay cho local player)
            playerCombat.TriggerAttackAnimationOnly();

            // Broadcast animation cho remote clients và server thấy
            if (Object.HasStateAuthority)
                RPC_PlayAttackAnimation(); // Host: gọi trực tiếp RPC vì đã là state auth
            else
                RPC_RequestAttackBroadcast(); // Client: gửi request lên server để broadcast
        }
    }

    public override void Render()
    {
        // Proxy nhận state từ host để animation / hướng nhìn bám theo network state.
        if (Object.HasInputAuthority)
            return;

        if (playerController != null)
        {
            playerController.MoveInput = NetworkedMoveInput;
            playerController.SetSprintInput(NetworkedSprintPressed);
            playerController.SetLastDirection(NetworkedLastDirection);
        }
    }

    /// <summary>
    /// Client (InputAuthority) g\u1eedi l\u00ean Server \u0111\u1ec3 server broadcast animation t\u1ea5n c\u00f4ng cho t\u1ea5t c\u1ea3.
    /// Tuy\u1ebft \u0111\u1ed1i kh\u00f4ng ch\u1ea1y damage \u1edf \u0111\u00e2y \u2014 damage \u0111\u01b0\u1ee3c x\u1eed l\u00fd ri\u00eang b\u1edfi InputAuthority qua RPC_RequestDamage.
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAttackBroadcast()
    {
        // Server nh\u1eadn request, broadcast animation cho t\u1ea5t c\u1ea3 remote peers
        RPC_PlayAttackAnimation();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAttackAnimation()
    {
        // Ch\u1ec9 ch\u1ea1y animation cho remote clients, kh\u00f4ng ch\u1ea1y cho local InputAuthority
        // (v\u00ec InputAuthority \u0111\u00e3 t\u1ef1 ch\u1ea1y animation tr\u01b0\u1edbc \u0111\u00f3)
        if (!Object.HasInputAuthority && playerCombat != null)
            playerCombat.TriggerAttackAnimationOnly();
    }
}
