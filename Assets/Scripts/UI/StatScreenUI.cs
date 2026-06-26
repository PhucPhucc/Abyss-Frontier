using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StatScreenUI : MonoBehaviour
{
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
        rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(480f, 680f); // Tăng chiều cao để đủ chỗ cho 6 stats

        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.06f, 0.94f);

        StatScreenUI screen = panelObject.GetComponent<StatScreenUI>();
        screen.Close();
        return screen;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
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

        BaseInputModule[] inputModules = eventSystem.GetComponents<BaseInputModule>();
        foreach (BaseInputModule module in inputModules)
        {
            if (module != null && module != inputModule)
            {
                if (Application.isPlaying)
                {
                    Destroy(module);
                }
                else
                {
                    DestroyImmediate(module);
                }
            }
        }
    }

    public void Open(PlayerStats stats)
    {
        EnsureEventSystem();
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

        if (buildDefaultLayoutIfEmpty && healthText == null)
        {
            BuildWideDefaultLayout();
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
        layout.spacing = 6f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Font font = GetDefaultFont();
        CreateLabel(rootPanel.transform, "CHI SO NHAN VAT", 22, FontStyle.Bold, TextAnchor.MiddleCenter, font, 34f);
        
        // Derived stats
        healthText = CreateLabel(rootPanel.transform, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 24f);
        staminaText = CreateLabel(rootPanel.transform, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 24f);
        attackText = CreateLabel(rootPanel.transform, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 24f);
        defenseText = CreateLabel(rootPanel.transform, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 24f);
        experienceText = CreateLabel(rootPanel.transform, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font, 24f);
        statPointsText = CreateLabel(rootPanel.transform, string.Empty, 16, FontStyle.Bold, TextAnchor.MiddleLeft, font, 26f);

        CreateSpacer(rootPanel.transform, 6f);
        CreateLabel(rootPanel.transform, "CHI SO CO BAN:", 15, FontStyle.Bold, TextAnchor.MiddleLeft, font, 22f);

        // Base stats labels
        strengthText = CreateLabel(rootPanel.transform, string.Empty, 15, FontStyle.Normal, TextAnchor.MiddleLeft, font, 20f);
        dexterityText = CreateLabel(rootPanel.transform, string.Empty, 15, FontStyle.Normal, TextAnchor.MiddleLeft, font, 20f);
        vitalityText = CreateLabel(rootPanel.transform, string.Empty, 15, FontStyle.Normal, TextAnchor.MiddleLeft, font, 20f);
        agilityText = CreateLabel(rootPanel.transform, string.Empty, 15, FontStyle.Normal, TextAnchor.MiddleLeft, font, 20f);
        enduranceText = CreateLabel(rootPanel.transform, string.Empty, 15, FontStyle.Normal, TextAnchor.MiddleLeft, font, 20f);
        intelligenceText = CreateLabel(rootPanel.transform, string.Empty, 15, FontStyle.Normal, TextAnchor.MiddleLeft, font, 20f);

        CreateSpacer(rootPanel.transform, 6f);

        // Buttons grid (we will just add them vertically for simplicity, styled neatly)
        upgradeStrengthButton = CreateButton(rootPanel.transform, "+ Suc manh (Strength)", font, () => Upgrade(StatType.Strength));
        upgradeDexterityButton = CreateButton(rootPanel.transform, "+ Kheo leo (Dexterity)", font, () => Upgrade(StatType.Dexterity));
        upgradeVitalityButton = CreateButton(rootPanel.transform, "+ Sinh luc (Vitality)", font, () => Upgrade(StatType.Vitality));
        upgradeAgilityButton = CreateButton(rootPanel.transform, "+ Nhanh nhen (Agility)", font, () => Upgrade(StatType.Agility));
        upgradeEnduranceButton = CreateButton(rootPanel.transform, "+ Ben bi (Endurance)", font, () => Upgrade(StatType.Endurance));
        upgradeIntelligenceButton = CreateButton(rootPanel.transform, "+ Tri luc (Intelligence)", font, () => Upgrade(StatType.Intelligence));

        CreateSpacer(rootPanel.transform, 6f);

        statusText = CreateLabel(rootPanel.transform, string.Empty, 14, FontStyle.Italic, TextAnchor.MiddleCenter, font, 36f);
        CreateLabel(rootPanel.transform, "Nhan [E] de dong", 13, FontStyle.Normal, TextAnchor.MiddleCenter, font, 22f);
    }

    private void BuildWideDefaultLayout()
    {
        VerticalLayoutGroup layout = rootPanel.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = rootPanel.AddComponent<VerticalLayoutGroup>();
        }

        layout.padding = new RectOffset(34, 34, 28, 28);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Font font = GetDefaultFont();
        CreateLabel(rootPanel.transform, "BASE CAMP - CHI SO NHAN VAT", 28, FontStyle.Bold, TextAnchor.MiddleCenter, font, 42f);

        Transform content = CreateHorizontalGroup(rootPanel.transform, "Stat Content", 1f, 0f, 24f);
        Transform statsColumn = CreateVerticalColumn(content, "Stats Left Column", 0.58f, 12f);
        Transform buttonsColumn = CreateVerticalColumn(content, "Buttons Right Column", 0.42f, 12f);

        CreateLabel(statsColumn, "THONG TIN CHIEN DAU", 19, FontStyle.Bold, TextAnchor.MiddleLeft, font, 30f);
        healthText = CreateLabel(statsColumn, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 30f);
        staminaText = CreateLabel(statsColumn, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 30f);
        attackText = CreateLabel(statsColumn, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 30f);
        defenseText = CreateLabel(statsColumn, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 30f);
        experienceText = CreateLabel(statsColumn, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font, 30f);
        statPointsText = CreateLabel(statsColumn, string.Empty, 19, FontStyle.Bold, TextAnchor.MiddleLeft, font, 34f);

        CreateSpacer(statsColumn, 10f);
        CreateLabel(statsColumn, "CHI SO CO BAN", 19, FontStyle.Bold, TextAnchor.MiddleLeft, font, 30f);
        strengthText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        dexterityText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        vitalityText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        agilityText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        enduranceText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
        intelligenceText = CreateLabel(statsColumn, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);

        CreateLabel(buttonsColumn, "NANG CAP CHI SO", 19, FontStyle.Bold, TextAnchor.MiddleLeft, font, 30f);
        upgradeStrengthButton = CreateButton(buttonsColumn, "+ Suc manh (Strength)", font, () => Upgrade(StatType.Strength));
        upgradeDexterityButton = CreateButton(buttonsColumn, "+ Kheo leo (Dexterity)", font, () => Upgrade(StatType.Dexterity));
        upgradeVitalityButton = CreateButton(buttonsColumn, "+ Sinh luc (Vitality)", font, () => Upgrade(StatType.Vitality));
        upgradeAgilityButton = CreateButton(buttonsColumn, "+ Nhanh nhen (Agility)", font, () => Upgrade(StatType.Agility));
        upgradeEnduranceButton = CreateButton(buttonsColumn, "+ Ben bi (Endurance)", font, () => Upgrade(StatType.Endurance));
        upgradeIntelligenceButton = CreateButton(buttonsColumn, "+ Tri luc (Intelligence)", font, () => Upgrade(StatType.Intelligence));

        CreateSpacer(buttonsColumn, 12f);
        statusText = CreateLabel(buttonsColumn, string.Empty, 17, FontStyle.Italic, TextAnchor.MiddleLeft, font, 42f);
        CreateLabel(buttonsColumn, "Nhan [E] de dong", 15, FontStyle.Normal, TextAnchor.MiddleLeft, font, 28f);
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

        layoutElement.preferredHeight = 48f;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        Text text = CreateLabel(buttonObject.transform, label, 16, FontStyle.Bold, TextAnchor.MiddleCenter, font, 46f);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private Transform CreateHorizontalGroup(Transform parent, string objectName, float flexibleHeight, float preferredHeight, float spacing)
    {
        GameObject groupObject = new GameObject(objectName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        groupObject.transform.SetParent(parent, false);

        HorizontalLayoutGroup group = groupObject.GetComponent<HorizontalLayoutGroup>();
        group.spacing = spacing;
        group.childAlignment = TextAnchor.UpperLeft;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = true;

        LayoutElement layoutElement = groupObject.GetComponent<LayoutElement>();
        layoutElement.flexibleHeight = flexibleHeight;
        layoutElement.preferredHeight = preferredHeight;

        return groupObject.transform;
    }

    private Transform CreateVerticalColumn(Transform parent, string objectName, float flexibleWidth, float spacing)
    {
        GameObject columnObject = new GameObject(objectName, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        columnObject.transform.SetParent(parent, false);

        VerticalLayoutGroup column = columnObject.GetComponent<VerticalLayoutGroup>();
        column.spacing = spacing;
        column.childAlignment = TextAnchor.UpperLeft;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;

        LayoutElement layoutElement = columnObject.GetComponent<LayoutElement>();
        layoutElement.flexibleWidth = flexibleWidth;

        return columnObject.transform;
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
