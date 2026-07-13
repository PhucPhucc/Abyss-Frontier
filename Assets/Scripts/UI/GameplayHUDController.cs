using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameplayHUDController : MonoBehaviour
{
    [Header("Runtime Layout")]
    [SerializeField] private bool buildDefaultLayoutIfEmpty = true;

    [Header("Text")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text hpText;
    [SerializeField] private Text staminaText;
    [SerializeField] private Text expText;
    [SerializeField] private Text statPointsText;

    [Header("Bars")]
    [SerializeField] private Image hpFill;
    [SerializeField] private Image staminaFill;
    [SerializeField] private Image expFill;

    private PlayerStats playerStats;
    private PlayerHealth playerHealth;
    private bool initialized;
    private bool subscribed;

    private static readonly Color PanelColor = new Color(0.05f, 0.06f, 0.07f, 0.88f);
    private static readonly Color HpColor = new Color(0.78f, 0.12f, 0.12f, 1f);
    private static readonly Color StaminaColor = new Color(0.14f, 0.62f, 0.24f, 1f);
    private static readonly Color ExpColor = new Color(0.22f, 0.42f, 0.92f, 1f);

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

    public static GameplayHUDController CreateRuntimeHud()
    {
        GameObject canvasObject = new GameObject(
            "Gameplay HUD Canvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(GameplayHUDController));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvasObject.GetComponent<GameplayHUDController>();
    }

    public void SetPlayer(PlayerStats stats)
    {
        EnsureInitialized();

        if (playerStats == stats)
        {
            Refresh();
            return;
        }

        Unsubscribe();
        playerStats = stats;
        playerHealth = playerStats != null ? playerStats.GetComponent<PlayerHealth>() : null;
        Subscribe();
        Refresh();
        Debug.Log($"[HUD] SetPlayer: stats={stats != null}, health={playerHealth != null}, hp={(playerHealth != null ? playerHealth.CurrentHealth : -1)}, max={(playerStats != null ? playerStats.MaxHealth : -1)}");
    }

    public void Refresh()
    {
        EnsureInitialized();

        if (playerStats == null)
        {
            SetText(levelText, "Level -");
            SetText(hpText, "HP -/-");
            SetText(staminaText, "ST -/-");
            SetText(expText, "EXP -/-");
            SetText(statPointsText, "Points -");
            SetFill(hpFill, 0f);
            SetFill(staminaFill, 0f);
            SetFill(expFill, 0f);
            return;
        }

        if (playerHealth == null)
        {
            playerHealth = playerStats.GetComponent<PlayerHealth>();
        }

        int currentHp = playerHealth != null ? playerHealth.CurrentHealth : playerStats.CurrentHealth;
        int maxHp = playerStats.MaxHealth;
        float currentStamina = playerStats.CurrentStamina;
        float maxStamina = playerStats.MaxStamina;
        float expFraction = playerStats.ExpToNextLevel <= 0
            ? 0f
            : playerStats.CurrentExp / (float)playerStats.ExpToNextLevel;

        SetText(levelText, $"Level {playerStats.Level}");
        SetText(hpText, $"HP {currentHp}/{maxHp}");
        SetText(staminaText, $"ST {Mathf.CeilToInt(currentStamina)}/{Mathf.CeilToInt(maxStamina)}");
        SetText(expText, $"EXP {playerStats.CurrentExp}/{playerStats.ExpToNextLevel}");
        SetText(statPointsText, $"Points {playerStats.StatPoints}");

        SetFill(hpFill, maxHp <= 0 ? 0f : currentHp / (float)maxHp);
        SetFill(staminaFill, maxStamina <= 0f ? 0f : currentStamina / maxStamina);
        SetFill(expFill, expFraction);
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        if (buildDefaultLayoutIfEmpty && hpText == null)
        {
            BuildDefaultLayout();
        }

        initialized = true;
    }

    private void Subscribe()
    {
        if (subscribed || playerStats == null || !isActiveAndEnabled)
        {
            return;
        }

        playerStats.StatsChanged += Refresh;
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += OnPlayerHealthChanged;
        }

        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed)
        {
            return;
        }

        if (playerStats != null)
        {
            playerStats.StatsChanged -= Refresh;
        }

        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnPlayerHealthChanged;
        }

        subscribed = false;
    }

    private void OnPlayerHealthChanged(int current, int max)
    {
        Refresh();
    }

    private void BuildDefaultLayout()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        Font font = GetDefaultFont();

        GameObject panelObject = new GameObject(
            "HUD Panel",
            typeof(RectTransform),
            typeof(Image),
            typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter));
        panelObject.transform.SetParent(transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(18f, -18f);
        panelRect.sizeDelta = new Vector2(360f, 142f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = PanelColor;

        VerticalLayoutGroup layout = panelObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 6f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        levelText = CreateLabel(panelObject.transform, "Level -", 18, FontStyle.Bold, font, 24f);
        hpFill = CreateBarRow(panelObject.transform, "HP -/-", HpColor, font, out hpText);
        staminaFill = CreateBarRow(panelObject.transform, "ST -/-", StaminaColor, font, out staminaText);
        expFill = CreateBarRow(panelObject.transform, "EXP -/-", ExpColor, font, out expText);
        statPointsText = CreateLabel(panelObject.transform, "Points -", 14, FontStyle.Normal, font, 20f);
    }

    private Image CreateBarRow(Transform parent, string label, Color fillColor, Font font, out Text rowText)
    {
        GameObject rowObject = new GameObject("HUD Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        rowObject.transform.SetParent(parent, false);

        LayoutElement rowElement = rowObject.GetComponent<LayoutElement>();
        rowElement.preferredHeight = 22f;

        HorizontalLayoutGroup rowLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 8f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = true;

        rowText = CreateLabel(rowObject.transform, label, 14, FontStyle.Bold, font, 22f);
        LayoutElement textElement = rowText.GetComponent<LayoutElement>();
        textElement.preferredWidth = 102f;

        GameObject barObject = new GameObject("Bar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        barObject.transform.SetParent(rowObject.transform, false);

        Image background = barObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.58f);

        LayoutElement barElement = barObject.GetComponent<LayoutElement>();
        barElement.flexibleWidth = 1f;
        barElement.preferredHeight = 16f;

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(barObject.transform, false);

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fill = fillObject.GetComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Simple;
        fill.fillAmount = 1f;

        return fill;
    }

    private Text CreateLabel(Transform parent, string value, int fontSize, FontStyle fontStyle, Font font, float preferredHeight)
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        labelObject.transform.SetParent(parent, false);

        Text text = labelObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        LayoutElement layoutElement = labelObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;

        return text;
    }

    private void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void SetFill(Image image, float value)
    {
        if (image != null)
        {
            float clampedValue = Mathf.Clamp01(value);
            if (image.type == Image.Type.Filled)
            {
                image.fillAmount = clampedValue;
            }
            else
            {
                RectTransform rectTransform = image.rectTransform;
                rectTransform.anchorMax = new Vector2(clampedValue, rectTransform.anchorMax.y);
                rectTransform.offsetMax = new Vector2(0f, rectTransform.offsetMax.y);
            }
        }
    }

    private Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }
}
