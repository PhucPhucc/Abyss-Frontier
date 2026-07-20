using UnityEngine;
using Fusion;

public enum SwitchType { Sun, Moon, Fire, Earth, Wind, Water }

[RequireComponent(typeof(InteractableTrigger))]
public class PuzzleSwitch : NetworkBehaviour, IInteractable
{
    public SwitchType myType;

    [Networked] private NetworkBool NetworkedActivated { get; set; }

    private ChangeDetector _changeDetector;
    private bool _localActivated = false;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // Late-join: nếu switch đã bật trước khi client join
        if (NetworkedActivated && !_localActivated)
            ActivateLocal();
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(NetworkedActivated))
            {
                if (NetworkedActivated && !_localActivated)
                    ActivateLocal();
                else if (!NetworkedActivated && _localActivated)
                    ResetLocal();
            }
        }
    }

    // ── IInteractable ────────────────────────────────────────────────────────────

    public void Interact(GameObject interactor)
    {
        // Nếu đã gạt rồi thì bỏ qua
        if (_localActivated) return;

        // Singleplayer: xử lý local
        if (Runner == null || Runner.GameMode == GameMode.Single)
        {
            _localActivated = true;
            if (anim != null)
            {
                Debug.Log("myType: " + myType);
                anim.SetBool("IsOn", true);
            }

            // Báo cho trọng tài biết nút này vừa được gạt
            if (PuzzleManager.Instance != null)
                PuzzleManager.Instance.OnSwitchActivated(myType, this);

            return;
        }

        // Multiplayer: gửi RPC lên Host
        // Dùng RpcSources.All vì scene objects không có InputAuthority
        if (Object.HasStateAuthority)
        {
            // Host tự xử lý trực tiếp
            if (NetworkedActivated) return;
            NetworkedActivated = true;
            // Thông báo PuzzleManager trên Host
            if (PuzzleManager.Instance != null)
                PuzzleManager.Instance.OnSwitchActivated(myType, this);
        }
        else
        {
            RPC_RequestActivate();
        }
    }

    public void ShowPrompt(bool show)
    {
        // Todo: Có thể gắn thêm UI hiển thị "[E] Gạt cần" ở đây giống như Torch
    }

    // ── RPCs ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Bất kỳ client nào gửi lên Host yêu cầu kích hoạt switch.
    /// Dùng RpcSources.All vì scene objects không có InputAuthority.
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestActivate()
    {
        if (NetworkedActivated) return;
        NetworkedActivated = true;

        // Host xử lý puzzle logic
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.OnSwitchActivated(myType, this);
    }

    // ── Hàm này để Manager gọi khi người chơi giải sai và cần gạt nảy lên lại ──

    public void ResetSwitch()
    {
        if (Runner == null || Runner.GameMode == GameMode.Single)
        {
            // Singleplayer
            ResetLocal();
            return;
        }

        // Multiplayer: chỉ Host mới có quyền reset
        if (Object.HasStateAuthority)
        {
            NetworkedActivated = false;
            // ChangeDetector sẽ trigger ResetLocal() trên tất cả clients
        }
    }

    // ── Local State ──────────────────────────────────────────────────────────────

    private void ActivateLocal()
    {
        _localActivated = true;
        if (anim != null)
        {
            Debug.Log("myType: " + myType);
            anim.SetBool("IsOn", true); // Kích hoạt hoạt ảnh gạt xuống
        }
    }

    private void ResetLocal()
    {
        _localActivated = false;
        if (anim != null)
        {
            anim.SetBool("IsOn", false); // Kích hoạt hoạt ảnh nảy lên
        }
    }
}