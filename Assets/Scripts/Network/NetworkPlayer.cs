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

    private float attackCooldownTimer;
    private float attackHitPendingTimer;

    private void Awake()
    {
        TryGetComponent(out playerController);
        TryGetComponent(out playerCombat);
        TryGetComponent(out playerStats);
        TryGetComponent(out playerHealth);
        TryGetComponent(out playerDash);
        TryGetComponent(out playerInput);
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

        // Vô hiệu hóa NetworkTransform khi chơi Singleplayer.
        // Nếu bật, NetworkTransform ghi đè transform.position mỗi frame render (Render interpolation),
        // làm xung đột và kéo giật Rigidbody2D đang di chuyển trong FixedUpdate.
        if (!isNetworkedMultiplayer)
        {
            if (TryGetComponent<NetworkTransform>(out var nt))
            {
                nt.enabled = false;
            }
        }

        if (playerInput != null)
            playerInput.enabled = !isNetworkedMultiplayer;

        if (playerCombat != null)
            playerCombat.UseNetworkInput = isNetworkedMultiplayer;

        if (Object.HasInputAuthority)
            AssignCameraTarget();
    }

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
            cam.Lens.OrthographicSize = 8f;

            var follow = camObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0, 0, -10);
            cam.Follow = transform;

            var brain = FindFirstObjectByType<CinemachineBrain>();
            if (brain == null)
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                    mainCam.gameObject.AddComponent<CinemachineBrain>();
            }
            return;
        }
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
        }

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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAttackAnimation()
    {
        if (playerCombat != null)
            playerCombat.TriggerAttackAnimationOnly();
    }
}
