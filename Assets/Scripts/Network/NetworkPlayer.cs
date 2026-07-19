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

    // Networked position để sync vị trí tới remote clients.
    // Dùng thay NetworkTransform vì NT conflict với Rigidbody2D interpolation.
    [Networked] private Vector2 NetworkedPosition { get; set; }

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
        bool isNetworkedMultiplayer = Runner != null &&
            Runner.GameMode != GameMode.Single &&
            Runner.GameMode != GameMode.Shared;

        // Luôn tắt NetworkTransform — nó ghi đè transform.position mỗi frame render
        // (Render interpolation), gây xung đột với Rigidbody2D đang di chuyển trong FixedUpdate.
        // Thay bằng sync vị trí thủ công qua [Networked] NetworkedPosition (bên dưới).
        if (TryGetComponent<NetworkTransform>(out var nt))
            nt.enabled = false;

        if (playerInput != null)
            playerInput.enabled = !isNetworkedMultiplayer;

        if (playerController != null)
            playerController.IsControlledByNetwork = isNetworkedMultiplayer;

        if (playerCombat != null)
            playerCombat.UseNetworkInput = isNetworkedMultiplayer;

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

            // Áp dụng velocity ngay trong Fusion tick (chỉ cho local input authority).
            // Remote clients sẽ nhận vị trí qua NetworkedPosition ở Render().
            if (!playerController.IsDashing)
                playerController.ApplyNetworkVelocity();
        }

        // Sync vị trí lên mạng sau khi di chuyển (chỉ State Authority mới ghi được)
        if (Object.HasStateAuthority && rb != null)
            NetworkedPosition = rb.position;

        if (input.IsDodgeSet && playerDash != null)
            playerDash.TryDash();

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Runner.DeltaTime;

        if (attackHitPendingTimer > 0f && Object.HasStateAuthority)
        {
            attackHitPendingTimer -= Runner.DeltaTime;
            if (attackHitPendingTimer <= 0f && playerCombat != null)
                playerCombat.TriggerAttackDamage();
        }

        if (input.IsAttackSet && attackCooldownTimer <= 0f && Object.HasStateAuthority)
        {
            attackCooldownTimer = 0.5f;
            attackHitPendingTimer = 0.2f;
            if (playerCombat != null)
                playerCombat.SetAttackCooldown(0.5f);

            RPC_PlayAttackAnimation();
        }
    }

    public override void Render()
    {
        // Chỉ áp dụng NetworkedPosition cho remote clients (không có input authority).
        // Local player tự di chuyển qua Rigidbody2D, không cần override position.
        if (!Object.HasInputAuthority && rb != null)
        {
            rb.position = NetworkedPosition;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAttackAnimation()
    {
        if (playerCombat != null)
            playerCombat.TriggerAttackAnimationOnly();
    }
}
