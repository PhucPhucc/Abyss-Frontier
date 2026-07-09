using UnityEngine;

/// <summary>
/// Quản lý UI Prompt hiện lên khi người chơi đến gần object.
/// Hỗ trợ hiệu ứng bobbing (lơ lửng).
/// </summary>
public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptCanvas; // Gán Inspector hoặc script tự tìm child có tên chứa "Prompt_Canvas"

    [Header("Animation")]
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobSpeed     = 2.2f;

    private bool  _visible;
    private float _baseLocalY;

    private void Awake()
    {
        // Tự tìm nếu chưa gán trong Inspector
        if (promptCanvas == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Prompt_Canvas"))
                {
                    promptCanvas = child.gameObject;
                    break;
                }
            }
        }

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
