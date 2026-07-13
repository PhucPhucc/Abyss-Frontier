using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(InteractableTrigger))]
public class InteractableLever : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private DoorController linkedDoor;
    [SerializeField] private Animator leverAnimator;

    private bool _isActivated = false;
    private InteractPromptUI promptUI;

    private void Awake()
    {
        promptUI = GetComponent<InteractPromptUI>();
    }

    public void Interact(GameObject interactor)
    {
        if (_isActivated) return;

        _isActivated = true;

        // Ẩn prompt
        if (promptUI != null)
            promptUI.SetVisible(false);

        StartCoroutine(CutsceneRoutine(interactor));
    }

    private IEnumerator CutsceneRoutine(GameObject player)
    {
        // 1. Khóa input và BẬT BẤT TỬ cho Player
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.DeactivateInput();

        PlayerHealth pHealth = player.GetComponent<PlayerHealth>();
        if (pHealth != null) pHealth.SetInvulnerable(true);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Animator playerAnim = player.GetComponentInChildren<Animator>();
        if (playerAnim != null) playerAnim.SetBool("isWalk", false);

        // 2. ĐÓNG BĂNG QUÁI VẬT (Tương tự Pause)
        EnemyAI[] enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.enabled = false; // Tắt AI (không đuổi theo nữa)
                enemy.StopAllCoroutines(); // Dừng các đòn chém dang dở
                
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null) enemyRb.linearVelocity = Vector2.zero; // Dừng di chuyển
                
                Animator eAnim = enemy.GetComponentInChildren<Animator>();
                if (eAnim != null) eAnim.speed = 0f; // Đóng băng hình ảnh quái
            }
        }

        // 3. Gạt cần và Lia Camera
        leverAnimator?.SetTrigger("Pull");

        // 2. Lia camera tới cánh cửa
        CinemachineCamera cam = Object.FindFirstObjectByType<CinemachineCamera>();
        Transform originalTarget = null;
        
        if (cam != null && linkedDoor != null)
        {
            originalTarget = cam.Target.TrackingTarget;
            cam.Target.TrackingTarget = linkedDoor.transform;
            
            // Chờ camera di chuyển tới cửa
            yield return new WaitForSeconds(1f);
        }

        // 3. Mở cửa và chờ animation mở cửa
        if (linkedDoor != null)
        {
            linkedDoor.OpenDoor();
            yield return new WaitForSeconds(1.0f); // Thời gian mở cửa
        }

        // 4. Lia camera trở lại người chơi
        if (cam != null && originalTarget != null)
        {
            cam.Target.TrackingTarget = originalTarget;
            yield return new WaitForSeconds(1f); // Chờ camera lia về
        }

        // 6. PHỤC HỒI LẠI TRẠNG THÁI GAME
        if (playerInput != null) playerInput.ActivateInput();
        if (pHealth != null) pHealth.SetInvulnerable(false);

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.enabled = true; // Bật lại AI
                Animator eAnim = enemy.GetComponentInChildren<Animator>();
                if (eAnim != null) eAnim.speed = 1f; // Chạy lại animation
            }
        }
    }

    public void ShowPrompt(bool show)
    {
        // Chỉ hiện nếu chưa bị gạt
        if (promptUI != null)
            promptUI.SetVisible(show && !_isActivated);
    }
}