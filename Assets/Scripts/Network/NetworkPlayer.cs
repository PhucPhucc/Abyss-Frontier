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

    // Networked state để sync vị trí và animation tới remote clients.
    [Networked] private Vector2 NetworkedPosition { get; set; }
    [Networked] private Vector2 NetworkedMoveInput { get; set; }
    [Networked] private Vector2 NetworkedLastDirection { get; set; }
    [Networked] private NetworkBool NetworkedSprintPressed { get; set; }
    [Networked] private NetworkBool NetworkedIsMoving { get; set; }

    // ── RPCs: Damage ──────────────────────────────────────────────────────────

    /// <summary>
    /// Host gửi damage về đúng client sở hữu player.
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

    // ── RPCs: Win (Boss chết) ─────────────────────────────────────────────────

    /// <summary>
    /// Host broadcast Victory UI sang tất cả client (kể cả Host tự hiển thị).
    /// Gọi từ BossController.TriggerVictoryUI() — chỉ chạy trên Host.
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowVictory()
    {
        BossVictoryUI vicUI = FindFirstObjectByType<BossVictoryUI>(FindObjectsInactive.Include);
        if (vicUI != null)
            vicUI.ShowVictory();
        else
            Debug.LogWarning("[NetworkPlayer] RPC_ShowVictory: BossVictoryUI không tìm thấy!");
    }

    // ── RPCs: Lose (Player chết) ──────────────────────────────────────────────

    /// <summary>
    /// Khi player chết cục bộ (InputAuthority), gửi lên Host để Host broadcast Lose cho tất cả.
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_NotifyPlayerDied()
    {
        // Host nhận được → broadcast Lose screen tới tất cả
        RPC_ShowLose();
    }

    /// <summary>
    /// Host broadcast màn hình Lose sang TẤT CẢ client.
    /// Cả người chết lẫn người còn sống đều thấy màn hình thua.
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowLose()
    {
        PlayerHealth localPlayerHealth = FindLocalPlayerHealth();
        DeathScreenUI screen = FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (screen != null)
            screen.ShowMultiplayerLose(localPlayerHealth);
        else
            Debug.LogWarning("[NetworkPlayer] RPC_ShowLose: DeathScreenUI không tìm thấy!");
    }

    // ── RPCs: Restart ─────────────────────────────────────────────────────────

    /// <summary>
    /// Bất kỳ client nào gửi lên Host để yêu cầu restart toàn session.
    /// Host sẽ relaunch, tất cả client tự kết nối lại.
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestRestart()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var launcher = FindFirstObjectByType<GameLauncher>();
        if (launcher != null)
            _ = launcher.LaunchAsHost(currentScene, GameSessionData.SessionName);
    }

    // ── Unity / Fusion Lifecycle ──────────────────────────────────────────────

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
        isMultiplayer = Runner != null &&
            Runner.GameMode != GameMode.Single &&
            Runner.GameMode != GameMode.Shared;

        // Tắt NetworkTransform — conflict với Rigidbody2D interpolation
        if (TryGetComponent<NetworkTransform>(out var nt))
            nt.enabled = false;

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
        {
            AssignCameraTarget();

            // Lắng nghe sự kiện chết của local player
            // Khi chết → gửi RPC lên Host → Host broadcast Lose cho tất cả
            if (isMultiplayer && playerHealth != null)
                playerHealth.Died += OnLocalPlayerDied;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (playerHealth != null)
            playerHealth.Died -= OnLocalPlayerDied;
    }

    // ── Death hook ────────────────────────────────────────────────────────────

    private void OnLocalPlayerDied()
    {
        // Chỉ InputAuthority (local player) gọi hàm này.
        // Gửi RPC lên Host để Host broadcast Lose screen.
        RPC_NotifyPlayerDied();
    }

    // ── Camera ────────────────────────────────────────────────────────────────

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

    // ── Fusion Ticks ──────────────────────────────────────────────────────────

    public override void FixedUpdateNetwork()
    {
        // Singleplayer / Shared: PlayerInput và Unity FixedUpdate tự xử lý
        if (Runner != null && (Runner.GameMode == GameMode.Single || Runner.GameMode == GameMode.Shared))
            return;

        if (!Object.HasInputAuthority)
            return;

        if (!GetInput<NetworkInputData>(out var input))
            return;

        // Áp input di chuyển
        if (playerController != null)
        {
            playerController.MoveInput = input.movement;
            playerController.SetSprintInput(input.IsSprintSet);

            if (!playerController.IsDashing)
                playerController.ApplyNetworkVelocity();
        }

        // Dodge
        if (input.IsDodgeSet && playerDash != null)
            playerDash.TryDash();

        // Attack cooldown timers
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Runner.DeltaTime;

        // Delayed attack damage
        if (attackHitPendingTimer > 0f)
        {
            attackHitPendingTimer -= Runner.DeltaTime;
            if (attackHitPendingTimer <= 0f && playerCombat != null)
            {
                Debug.Log("[NetworkPlayer] TriggerAttackDamage trên InputAuthority");
                playerCombat.TriggerAttackDamage(isHostPlayer: Object.HasStateAuthority);
            }
        }

        // Attack trigger
        if (input.IsAttackSet && attackCooldownTimer <= 0f)
        {
            attackCooldownTimer = 0.5f;
            attackHitPendingTimer = 0.2f;
            if (playerCombat != null)
                playerCombat.SetAttackCooldown(0.5f);

            playerCombat.TriggerAttackAnimationOnly();

            if (Object.HasStateAuthority)
                RPC_PlayAttackAnimation();
            else
                RPC_RequestAttackBroadcast();
        }

        // Sync vị trí và animation state lên network
        if (Object.HasStateAuthority && rb != null)
        {
            NetworkedPosition = rb.position;
            if (playerController != null)
            {
                NetworkedMoveInput = playerController.MoveInput;
                NetworkedLastDirection = playerController.LastDirection;
                NetworkedSprintPressed = playerController.IsSprinting;
                NetworkedIsMoving = playerController.IsMoving;
            }
        }
        else if (!Object.HasStateAuthority && rb != null)
        {
            RPC_SyncPositionToHost(rb.position,
                playerController != null ? playerController.MoveInput : Vector2.zero,
                playerController != null ? playerController.LastDirection : Vector2.zero,
                playerController != null && playerController.IsSprinting,
                playerController != null && playerController.IsMoving);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SyncPositionToHost(Vector2 position, Vector2 moveInput, Vector2 lastDirection,
        NetworkBool isSprinting, NetworkBool isMoving)
    {
        NetworkedPosition = position;
        NetworkedMoveInput = moveInput;
        NetworkedLastDirection = lastDirection;
        NetworkedSprintPressed = isSprinting;
        NetworkedIsMoving = isMoving;
    }

    public override void Render()
    {
        // Remote proxy: nhận state từ host để animation / hướng nhìn bám theo network state.
        if (Object.HasInputAuthority)
            return;

        if (rb != null)
            rb.position = NetworkedPosition;

        if (playerController != null)
        {
            playerController.MoveInput = NetworkedMoveInput;
            playerController.SetSprintInput(NetworkedSprintPressed);
            playerController.SetLastDirection(NetworkedLastDirection);
        }
    }

    // ── Attack RPCs ───────────────────────────────────────────────────────────

    /// <summary>
    /// Client (InputAuthority) gửi lên Server để server broadcast animation tấn công cho tất cả.
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAttackBroadcast()
    {
        RPC_PlayAttackAnimation();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAttackAnimation()
    {
        // Chỉ chạy animation cho remote clients, không chạy cho local InputAuthority
        if (!Object.HasInputAuthority && playerCombat != null)
            playerCombat.TriggerAttackAnimationOnly();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static NetworkPlayer FindLocalNetworkPlayer()
    {
        NetworkPlayer[] all = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        foreach (var np in all)
        {
            if (np.Object != null && np.Object.HasInputAuthority)
                return np;
        }
        return null;
    }

    private static PlayerHealth FindLocalPlayerHealth()
    {
        NetworkPlayer local = FindLocalNetworkPlayer();
        if (local != null && local.playerHealth != null)
            return local.playerHealth;

        // Fallback: tìm PlayerHealth đầu tiên
        return FindFirstObjectByType<PlayerHealth>();
    }
}
