using UnityEngine;

/// <summary>
/// Tìm child "TorchPrompt_Canvas" đã có sẵn trong prefab và toggle SetActive.
/// Gắn vào cùng GameObject chứa Torch.cs.
/// </summary>
[RequireComponent(typeof(Torch))]
public class TorchPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptCanvas; // kéo TorchPrompt_Canvas vào đây, hoặc để tự tìm

    [Header("Animation")]
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobSpeed     = 2.2f;

    private bool  _visible;
    private float _baseLocalY;

    private void Awake()
    {
        // Tự tìm nếu chưa gán trong Inspector
        if (promptCanvas == null)
            promptCanvas = transform.Find("TorchPrompt_Canvas")?.gameObject;

        if (promptCanvas != null)
            _baseLocalY = promptCanvas.transform.localPosition.y;

        SetVisible(false);
    }

    private void Update()
    {
        if (!_visible || promptCanvas == null) return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        Vector3 pos = promptCanvas.transform.localPosition;
        promptCanvas.transform.localPosition = new Vector3(pos.x, _baseLocalY + bob, pos.z);
    }

    public void SetVisible(bool show)
    {
        _visible = show;
        if (promptCanvas != null)
            promptCanvas.SetActive(show);
    }
}
