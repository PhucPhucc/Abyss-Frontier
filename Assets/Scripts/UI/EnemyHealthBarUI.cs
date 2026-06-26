using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.05f, 0f);

    [Header("Runtime Layout")]
    [SerializeField] private bool buildDefaultLayoutIfEmpty = true;
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Image fillImage;

    private bool initialized;
    private bool subscribed;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void LateUpdate()
    {
        if (worldCanvas != null && worldCanvas.worldCamera == null)
        {
            worldCanvas.worldCamera = Camera.main;
        }
    }

    public void SetTarget(EnemyHealth health)
    {
        EnsureInitialized();

        if (enemyHealth == health)
        {
            Refresh();
            return;
        }

        Unsubscribe();
        enemyHealth = health;
        Subscribe();
        Refresh();
    }

    public void Refresh()
    {
        EnsureInitialized();

        if (enemyHealth == null || enemyHealth.IsDead)
        {
            SetVisible(false);
            return;
        }

        float fraction = enemyHealth.HealthFraction;
        if (fillImage != null)
        {
            SetFill(fraction);
        }

        SetVisible(!hideWhenFull || fraction < 1f);
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>() ?? GetComponentInParent<EnemyHealth>();
        }

        if (buildDefaultLayoutIfEmpty && fillImage == null)
        {
            BuildDefaultLayout();
        }

        initialized = true;
    }

    private void Subscribe()
    {
        if (subscribed || enemyHealth == null || !isActiveAndEnabled)
        {
            return;
        }

        enemyHealth.HealthChanged += OnHealthChanged;
        enemyHealth.Died += OnDied;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || enemyHealth == null)
        {
            return;
        }

        enemyHealth.HealthChanged -= OnHealthChanged;
        enemyHealth.Died -= OnDied;
        subscribed = false;
    }

    private void OnHealthChanged(int current, int max)
    {
        Refresh();
    }

    private void OnDied()
    {
        SetVisible(false);
    }

    private void BuildDefaultLayout()
    {
        GameObject canvasObject = new GameObject("Enemy HP Canvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = worldOffset;
        canvasObject.transform.localScale = Vector3.one * 0.01f;

        worldCanvas = canvasObject.GetComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.worldCamera = Camera.main;
        worldCanvas.sortingOrder = 20;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(92f, 12f);

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(canvasObject.transform, false);

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.7f);

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(backgroundObject.transform, false);

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        fillImage = fillObject.GetComponent<Image>();
        fillImage.color = new Color(0.82f, 0.1f, 0.1f, 1f);
        fillImage.type = Image.Type.Simple;
        fillImage.fillAmount = 1f;
    }

    private void SetFill(float value)
    {
        float clampedValue = Mathf.Clamp01(value);
        RectTransform rectTransform = fillImage.rectTransform;
        rectTransform.anchorMax = new Vector2(clampedValue, rectTransform.anchorMax.y);
        rectTransform.offsetMax = new Vector2(0f, rectTransform.offsetMax.y);
        fillImage.fillAmount = 1f;
    }

    private void SetVisible(bool visible)
    {
        if (worldCanvas != null)
        {
            worldCanvas.enabled = visible;
        }
    }
}
