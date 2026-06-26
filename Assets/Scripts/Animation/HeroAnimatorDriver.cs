using UnityEngine;

public class HeroAnimatorDriver : CharacterAnimationHandler
{
    private Animator animator;
    private CharacterMotor motor;
    private PlayerStats playerStats;
    private PlayerController playerController;
    private PlayerDash playerDash;

    [Header("Cấu hình Combo & Tấn công")]
    [Tooltip("Khoảng thời gian tối đa giữa 2 lần bấm để được tính là combo (giây).")]
    [SerializeField] private float comboWindow = 1.2f;

    [Tooltip("Khoảng thời gian chờ tối thiểu giữa các lần bấm để tránh bị khựng/spam quá nhanh.")]
    [SerializeField] private float attackInputDelay = 0.15f;

    private int comboStep = 0;              // 0: Idle/Run, 1: Đòn 1, 2: Đòn 2
    private float lastAttackTime;           // Thời điểm bấm nút tấn công hợp lệ gần nhất

    private void Awake()
    {
        animator = GetComponent<Animator>();
        motor = GetComponent<CharacterMotor>();
        playerStats = GetComponent<PlayerStats>();
        playerController = GetComponent<PlayerController>();
        playerDash = GetComponent<PlayerDash>();
    }

    private void Update()
    {
        if (animator == null || motor == null)
            return;

        // Xử lý trạng thái chết
        if (playerStats != null && playerStats.IsDead)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            return;
        }

        // Cập nhật các tham số hướng di chuyển cho Blend Tree
        animator.SetFloat("moveX", motor.LastDirection.x);
        animator.SetFloat("moveY", motor.LastDirection.y);
        animator.SetFloat("lastMoveX", motor.LastDirection.x);
        animator.SetFloat("lastMoveY", motor.LastDirection.y);


        // XỬ LÝ LOGIC DI CHUYỂN (WALK & RUN)
        bool isMoving = motor.IsMoving;
        bool isSprinting = playerController != null && playerController.IsSprinting;

        // Đi bộ: Khi có di chuyển nhưng KHÔNG bấm giữ nút chạy nhanh
        animator.SetBool("isWalking", isMoving && !isSprinting);

        // Chạy nhanh: Khi có di chuyển VÀ có bấm giữ nút chạy nhanh
        animator.SetBool("isRunning", isMoving && isSprinting);

        // TỰ ĐỘNG RESET COMBO: Nếu người chơi đứng yên quá thời gian comboWindow, đưa combo về ban đầu
        if (comboStep > 0 && Time.time - lastAttackTime > comboWindow)
        {
            ResetCombo();
        }
    }

    public override void TriggerAttack()
    {
        if (animator == null) return;

        float timeSinceLastAttack = Time.time - lastAttackTime;

        // Chống spam: Nếu bấm quá nhanh (chưa đủ thời gian delay) thì bỏ qua lệnh này
        if (comboStep > 0 && timeSinceLastAttack < attackInputDelay)
        {
            return;
        }

        // Cập nhật lại thời gian bấm nút hợp lệ
        lastAttackTime = Time.time;

        // Lần đầu tiên bấm HOẶC đã quá thời gian chờ combo -> Đánh đòn 1
        if (comboStep == 0 || timeSinceLastAttack > comboWindow)
        {
            comboStep = 1;
            animator.SetTrigger("attack");
        }
        // Bấm lần 2 hợp lệ trong khung giờ combo Window -> Kích hoạt đòn 2
        else if (comboStep == 1 && timeSinceLastAttack <= comboWindow)
        {
            comboStep = 2;
            animator.SetTrigger("attack2");
        }
        // Nếu tiếp tục bấm ở đòn 2 -> Quay vòng lại đòn 1
        else if (comboStep == 2)
        {
            comboStep = 1;
            animator.SetTrigger("attack");
        }
    }

    public override void TriggerHurt()
    {
        if (animator == null) return;

        // Khi bị trúng đòn, lập tức bẻ gãy tiến trình combo hiện tại để diễn hoạt ảnh bị đau
        ResetCombo();

        // Kích hoạt trigger trúng đòn trong Animator
        animator.SetTrigger("hurt");
    }

    public override void TriggerDeath()
    {
        if (animator == null) return;

        // Reset toàn bộ trạng thái để đảm bảo hoạt ảnh chết chuẩn xác nhất
        ResetCombo();

        animator.SetTrigger("death");
    }

    public void ResetCombo()
    {
        comboStep = 0;
    }

    public override void TriggerRespawn()
    {
        if (animator != null)
            animator.speed = 1f;
    }
}
