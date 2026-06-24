using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StatScreenUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private bool buildDefaultLayoutIfEmpty = true;

    [Header("Stat Labels")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text staminaText;
    [SerializeField] private Text attackText;
    [SerializeField] private Text defenseText;
    [SerializeField] private Text experienceText;
    [SerializeField] private Text statPointsText;
    [SerializeField] private Text statusText;

    [Header("Upgrade Buttons")]
    [SerializeField] private Button upgradeHealthButton;
    [SerializeField] private Button upgradeStaminaButton;
    [SerializeField] private Button upgradeAttackButton;
    [SerializeField] private Button upgradeDefenseButton;

    private PlayerStats playerStats;
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

    public static StatScreenUI CreateRuntimeScreen()
    {
        GameObject canvasObject = new GameObject(
            "Stat Screen Canvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject(
            "Stat Screen UI",
            typeof(RectTransform),
            typeof(Image),
            typeof(VerticalLayoutGroup),
            typeof(StatScreenUI));
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(430f, 510f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.06f, 0.92f);

        StatScreenUI screen = panelObject.GetComponent<StatScreenUI>();
        screen.Close();
        return screen;
    }

    public void Open(PlayerStats stats)
    {
        EnsureInitialized();
        SetPlayerStats(stats);
        rootPanel.SetActive(true);
        Subscribe();
        Refresh();
    }

    public void Close()
    {
        EnsureInitialized();
        rootPanel.SetActive(false);
    }

    public void SetPlayerStats(PlayerStats stats)
    {
        EnsureInitialized();

        if (playerStats == stats)
        {
            return;
        }

        Unsubscribe();
        playerStats = stats;
        Subscribe();
        Refresh();
    }

    public void Refresh()
    {
        EnsureInitialized();

        if (playerStats == null)
        {
            SetText(statusText, "Khong tim thay PlayerStats.");
            SetUpgradeButtonsInteractable(false);
            return;
        }

        SetText(healthText, $"Mau: {playerStats.CurrentHealth}/{playerStats.MaxHealth}");
        SetText(staminaText, $"The luc: {Mathf.CeilToInt(playerStats.CurrentStamina)}/{Mathf.CeilToInt(playerStats.MaxStamina)}");
        SetText(attackText, $"Sat thuong: {playerStats.AttackDamage}");
        SetText(defenseText, $"Phong thu: {playerStats.Defense}");
        SetText(experienceText, $"EXP: {playerStats.CurrentExperience}/{playerStats.ExperiencePerStatPoint}");
        SetText(statPointsText, $"Diem nang cap: {playerStats.StatPoints}");

        bool canUpgrade = playerStats.StatPoints > 0;
        SetText(statusText, canUpgrade ? "Chon chi so de nang cap." : "Can them EXP de co diem nang cap.");
        SetUpgradeButtonsInteractable(canUpgrade);
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        if (rootPanel == null)
        {
            rootPanel = gameObject;
        }

        if (buildDefaultLayoutIfEmpty && healthText == null)
        {
            BuildDefaultLayout();
        }

        WireButtons();
        initialized = true;
    }

    private void BuildDefaultLayout()
    {
        VerticalLayoutGroup layout = rootPanel.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = rootPanel.AddComponent<VerticalLayoutGroup>();
        }

        layout.padding = new RectOffset(18, 18, 18, 18);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Font font = GetDefaultFont();
        CreateLabel(rootPanel.transform, "CHI SO NHAN VAT", 24, FontStyle.Bold, TextAnchor.MiddleCenter, font, 38f);
        healthText = CreateLabel(rootPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        staminaText = CreateLabel(rootPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        attackText = CreateLabel(rootPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        defenseText = CreateLabel(rootPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        experienceText = CreateLabel(rootPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        statPointsText = CreateLabel(rootPanel.transform, string.Empty, 18, FontStyle.Bold, TextAnchor.MiddleLeft, font, 30f);

        CreateSpacer(rootPanel.transform, 8f);
        upgradeHealthButton = CreateButton(rootPanel.transform, "+ Mau", font, () => Upgrade(PlayerStatType.MaxHealth));
        upgradeStaminaButton = CreateButton(rootPanel.transform, "+ The luc", font, () => Upgrade(PlayerStatType.MaxStamina));
        upgradeAttackButton = CreateButton(rootPanel.transform, "+ Sat thuong", font, () => Upgrade(PlayerStatType.Attack));
        upgradeDefenseButton = CreateButton(rootPanel.transform, "+ Phong thu", font, () => Upgrade(PlayerStatType.Defense));
        CreateSpacer(rootPanel.transform, 8f);

        statusText = CreateLabel(rootPanel.transform, string.Empty, 15, FontStyle.Italic, TextAnchor.MiddleCenter, font, 44f);
        CreateLabel(rootPanel.transform, "Nhan [E] de dong", 14, FontStyle.Normal, TextAnchor.MiddleCenter, font, 24f);
    }

    private void WireButtons()
    {
        WireButton(upgradeHealthButton, () => Upgrade(PlayerStatType.MaxHealth));
        WireButton(upgradeStaminaButton, () => Upgrade(PlayerStatType.MaxStamina));
        WireButton(upgradeAttackButton, () => Upgrade(PlayerStatType.Attack));
        WireButton(upgradeDefenseButton, () => Upgrade(PlayerStatType.Defense));
    }

    private void WireButton(Button button, UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void Upgrade(PlayerStatType statType)
    {
        if (playerStats != null && playerStats.TryUpgradeStat(statType))
        {
            Refresh();
        }
    }

    private void Subscribe()
    {
        if (subscribed || playerStats == null || !isActiveAndEnabled)
        {
            return;
        }

        playerStats.StatsChanged += Refresh;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || playerStats == null)
        {
            return;
        }

        playerStats.StatsChanged -= Refresh;
        subscribed = false;
    }

    private void SetUpgradeButtonsInteractable(bool interactable)
    {
        SetInteractable(upgradeHealthButton, interactable);
        SetInteractable(upgradeStaminaButton, interactable);
        SetInteractable(upgradeAttackButton, interactable);
        SetInteractable(upgradeDefenseButton, interactable);
    }

    private void SetInteractable(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private Text CreateLabel(
        Transform parent,
        string value,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Font font,
        float preferredHeight)
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        labelObject.transform.SetParent(parent, false);

        Text text = labelObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        LayoutElement layoutElement = labelObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;

        return text;
    }

    private Button CreateButton(Transform parent, string label, Font font, UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.22f, 0.24f, 1f);

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 36f;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        Text text = CreateLabel(buttonObject.transform, label, 16, FontStyle.Bold, TextAnchor.MiddleCenter, font, 34f);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private void CreateSpacer(Transform parent, float preferredHeight)
    {
        GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(parent, false);
        spacer.GetComponent<LayoutElement>().preferredHeight = preferredHeight;
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
