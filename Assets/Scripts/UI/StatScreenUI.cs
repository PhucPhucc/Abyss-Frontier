using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StatScreenUI : MonoBehaviour
{
    private const float PopupScreenFill = 0.8f;
    private const float PopupScreenInset = (1f - PopupScreenFill) * 0.5f;

    [Header("Root")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private bool buildDefaultLayoutIfEmpty = true;

    [Header("Stat Labels (Derived)")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text staminaText;
    [SerializeField] private Text attackText;
    [SerializeField] private Text defenseText; // Dùng để hiển thị Né tránh & Tốc độ
    [SerializeField] private Text experienceText;
    [SerializeField] private Text statPointsText;
    [SerializeField] private Text statusText;

    [Header("Base Stat Labels")]
    [SerializeField] private Text strengthText;
    [SerializeField] private Text dexterityText;
    [SerializeField] private Text vitalityText;
    [SerializeField] private Text agilityText;
    [SerializeField] private Text enduranceText;
    [SerializeField] private Text intelligenceText;

    [Header("Old Upgrade Buttons (Compatibility)")]
    [SerializeField] private Button upgradeHealthButton;
    [SerializeField] private Button upgradeStaminaButton;
    [SerializeField] private Button upgradeAttackButton;
    [SerializeField] private Button upgradeDefenseButton;

    [Header("New Upgrade Buttons")]
    [SerializeField] private Button upgradeStrengthButton;
    [SerializeField] private Button upgradeDexterityButton;
    [SerializeField] private Button upgradeVitalityButton;
    [SerializeField] private Button upgradeAgilityButton;
    [SerializeField] private Button upgradeEnduranceButton;
    [SerializeField] private Button upgradeIntelligenceButton;

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
        EnsureEventSystem();

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
        rectTransform.sizeDelta = new Vector2(480f, 680f); // Tăng chiều cao để đủ chỗ cho 6 stats

        ApplyExpandedLayout(rectTransform);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.06f, 0.94f);

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

        SetText(healthText, $"Mau (HP): {playerStats.CurrentHealth}/{playerStats.MaxHealth}");
        SetText(staminaText, $"The luc: {Mathf.CeilToInt(playerStats.CurrentStamina)}/{Mathf.CeilToInt(playerStats.MaxStamina)}");
        SetText(attackText, $"Sat thuong (ATK): {playerStats.AttackDamage}");
        SetText(defenseText, $"Ne tranh: {playerStats.DodgeChance * 100:F0}%  |  Toc do: {playerStats.MoveSpeed:F2}");
        SetText(experienceText, $"EXP: {playerStats.CurrentExp}/{playerStats.ExpToNextLevel} (Heso: x{playerStats.ExpMultiplier:F1})");
        SetText(statPointsText, $"Diem nang cap: {playerStats.StatPoints}");

        // Base Stats
        SetText(strengthText, $"  Suc manh (Strength): {playerStats.Strength}");
        SetText(dexterityText, $"  Kheo leo (Dexterity): {playerStats.Dexterity}");
        SetText(vitalityText, $"  Sinh luc (Vitality): {playerStats.Vitality}");
        SetText(agilityText, $"  Nhanh nhẹn (Agility): {playerStats.Agility}");
        SetText(enduranceText, $"  Ben bi (Endurance): {playerStats.Endurance}");
        SetText(intelligenceText, $"  Tri luc (Intelligence): {playerStats.Intelligence}");

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

        ApplyExpandedLayout(rootPanel.GetComponent<RectTransform>());

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

        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        Font font = GetDefaultFont();
        CreateLabel(rootPanel.transform, "CHI SO NHAN VAT", 26, FontStyle.Bold, TextAnchor.MiddleCenter, font, 42f);

        Transform contentRow = CreateContentRow(rootPanel.transform);
        Transform statsColumn = CreateColumn(contentRow, "Stats Column", 3f);
        Transform upgradeColumn = CreateColumn(contentRow, "Upgrade Column", 2f);
        
        CreateLabel(statsColumn, "THONG TIN HIEN TAI", 18, FontStyle.Bold, TextAnchor.MiddleLeft, font, 30f);
        healthText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        staminaText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        attackText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        defenseText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        experienceText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        statPointsText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Bold, TextAnchor.MiddleLeft, font, 30f);

        CreateSpacer(statsColumn, 10f);
        CreateLabel(statsColumn, "CHI SO CO BAN", 17, FontStyle.Bold, TextAnchor.MiddleLeft, font, 28f);
        strengthText = CreateLabel(statsColumn, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 26f);
        dexterityText = CreateLabel(statsColumn, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 26f);
        vitalityText = CreateLabel(statsColumn, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 26f);
        agilityText = CreateLabel(statsColumn, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 26f);
        enduranceText = CreateLabel(statsColumn, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 26f);
        intelligenceText = CreateLabel(statsColumn, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 26f);

        CreateLabel(upgradeColumn, "TANG CHI SO", 18, FontStyle.Bold, TextAnchor.MiddleCenter, font, 32f);
        upgradeStrengthButton = CreateButton(upgradeColumn, "+ Suc manh (Strength)", font, () => Upgrade(StatType.Strength));
        upgradeDexterityButton = CreateButton(upgradeColumn, "+ Kheo leo (Dexterity)", font, () => Upgrade(StatType.Dexterity));
        upgradeVitalityButton = CreateButton(upgradeColumn, "+ Sinh luc (Vitality)", font, () => Upgrade(StatType.Vitality));
        upgradeAgilityButton = CreateButton(upgradeColumn, "+ Nhanh nhen (Agility)", font, () => Upgrade(StatType.Agility));
        upgradeEnduranceButton = CreateButton(upgradeColumn, "+ Ben bi (Endurance)", font, () => Upgrade(StatType.Endurance));
        upgradeIntelligenceButton = CreateButton(upgradeColumn, "+ Tri luc (Intelligence)", font, () => Upgrade(StatType.Intelligence));

        CreateSpacer(upgradeColumn, 12f);
        statusText = CreateLabel(upgradeColumn, string.Empty, 15, FontStyle.Italic, TextAnchor.MiddleCenter, font, 42f);
        CreateLabel(upgradeColumn, "Nhan [E] de dong", 14, FontStyle.Normal, TextAnchor.MiddleCenter, font, 24f);
    }

    private void WireButtons()
    {
        // Tương thích ngược với các nút cũ nếu được gán trong prefab
        WireButton(upgradeHealthButton, () => Upgrade(StatType.Vitality));
        WireButton(upgradeStaminaButton, () => Upgrade(StatType.Endurance));
        WireButton(upgradeAttackButton, () => Upgrade(StatType.Strength));
        WireButton(upgradeDefenseButton, () => Upgrade(StatType.Dexterity));

        // Các nút mới
        WireButton(upgradeStrengthButton, () => Upgrade(StatType.Strength));
        WireButton(upgradeDexterityButton, () => Upgrade(StatType.Dexterity));
        WireButton(upgradeVitalityButton, () => Upgrade(StatType.Vitality));
        WireButton(upgradeAgilityButton, () => Upgrade(StatType.Agility));
        WireButton(upgradeEnduranceButton, () => Upgrade(StatType.Endurance));
        WireButton(upgradeIntelligenceButton, () => Upgrade(StatType.Intelligence));
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

    private void Upgrade(StatType statType)
    {
        if (playerStats != null && playerStats.AllocateStat(statType))
        {
            PlayerController controller = playerStats.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.RefreshStats();
            }

            Refresh();
        }
    }

    private static void ApplyExpandedLayout(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = new Vector2(PopupScreenInset, PopupScreenInset);
        rectTransform.anchorMax = new Vector2(1f - PopupScreenInset, 1f - PopupScreenInset);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = FindFirstObjectByType<EventSystem>();
        }

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (inputModule.actionsAsset == null)
        {
            inputModule.AssignDefaultActions();
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

        SetInteractable(upgradeStrengthButton, interactable);
        SetInteractable(upgradeDexterityButton, interactable);
        SetInteractable(upgradeVitalityButton, interactable);
        SetInteractable(upgradeAgilityButton, interactable);
        SetInteractable(upgradeEnduranceButton, interactable);
        SetInteractable(upgradeIntelligenceButton, interactable);
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

    private Transform CreateContentRow(Transform parent)
    {
        GameObject rowObject = new GameObject(
            "Stat Screen Content Row",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup),
            typeof(LayoutElement));
        rowObject.transform.SetParent(parent, false);

        HorizontalLayoutGroup layout = rowObject.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 22f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        LayoutElement layoutElement = rowObject.GetComponent<LayoutElement>();
        layoutElement.flexibleHeight = 1f;
        layoutElement.flexibleWidth = 1f;

        return rowObject.transform;
    }

    private Transform CreateColumn(Transform parent, string name, float flexibleWidth)
    {
        GameObject columnObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(VerticalLayoutGroup),
            typeof(LayoutElement));
        columnObject.transform.SetParent(parent, false);

        VerticalLayoutGroup layout = columnObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 4, 4);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        LayoutElement layoutElement = columnObject.GetComponent<LayoutElement>();
        layoutElement.flexibleWidth = flexibleWidth;
        layoutElement.flexibleHeight = 1f;

        return columnObject.transform;
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
        layoutElement.preferredHeight = 30f; // Nhỏ hơn chút cho vừa màn hình

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        Text text = CreateLabel(buttonObject.transform, label, 14, FontStyle.Bold, TextAnchor.MiddleCenter, font, 28f);
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
