using UnityEngine;
using UnityEngine.Rendering.Universal;
using Fusion;

public class Torch : NetworkBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private Light2D light2D;

    // Networked state — ChangeDetector sẽ phát hiện thay đổi trong Render() cho tất cả clients
    [Networked]
    private NetworkBool NetworkedLit { get; set; }

    private ChangeDetector _changeDetector;

    private bool _localLit = false;
    private InteractPromptUI promptUI;

    private void Awake()
    {
        promptUI = GetComponent<InteractPromptUI>();
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(NetworkedLit) && NetworkedLit)
                LightTorchLocal();
        }
    }

    // ── IInteractable ────────────────────────────────────────────────────────────

    public void Interact(GameObject interactor)
    {
        if (_localLit) return;

        // Singleplayer: xử lý local
        if (Runner == null || Runner.GameMode == GameMode.Single)
        {
            LightTorchLocal();
            return;
        }

        // Multiplayer: gửi RPC lên Host
        RPC_RequestLight();
    }

    public void ShowPrompt(bool show)
    {
        if (promptUI == null) return;
        promptUI.SetVisible(show && !_localLit);
    }

    // ── RPCs ────────────────────────────────────────────────────────────────────

    /// <summary>Bất kỳ client nào gửi lên Host yêu cầu thắp đuốc.
    /// Dùng RpcSources.All vì scene objects không có InputAuthority.</summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestLight()
    {
        if (NetworkedLit) return;
        NetworkedLit = true; // ChangeDetector sẽ tự trigger Render() trên tất cả clients
    }

    // ── Local State ──────────────────────────────────────────────────────────────

    public void LightTorchLocal()
    {
        if (_localLit) return;
        _localLit = true;

        if (animator != null)
            animator.SetBool("Lit", true);

        if (light2D != null)
        {
            light2D.intensity = 1.8f;
            light2D.enabled = true;
        }

        if (promptUI != null)
            promptUI.SetVisible(false);
    }

    public bool IsLit() => _localLit;
}