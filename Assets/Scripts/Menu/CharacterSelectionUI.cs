using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CharacterSelectionUI : MonoBehaviour
{
    private static readonly string[] DefaultCharacterNames =
    {
        "Hero 1",
        "Player",
        "SPUM Hero",
        "Player 2"
    };

    [Header("Root")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private bool buildDefaultLayoutIfEmpty = true;

    [Header("Character Options")]
    [SerializeField] private string[] characterNames = DefaultCharacterNames;
    [SerializeField] private Sprite[] characterPortraits;
    [SerializeField] private Button[] characterButtons;
    [SerializeField] private Image[] characterButtonImages;
    [SerializeField] private Text[] characterLabels;
    [SerializeField] private Image[] portraitImages;

    [Header("Navigation")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Text statusText;

    [Header("Flow")]
    [SerializeField] private MenuFlowController flowController;

    private bool initialized;
    private int selectedCharacterIndex = -1;

    public int OptionCount => characterButtons != null && characterButtons.Length > 0
        ? characterButtons.Length
        : GetCharacterNames().Length;

    public int SelectedCharacterIndex => selectedCharacterIndex;
    public bool CanStart => selectedCharacterIndex >= 0 && startButton != null && startButton.interactable;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        EnsureInitialized();
        ResetSelection();
    }

    public static CharacterSelectionUI CreateRuntimeCanvas(MenuFlowController controller = null)
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject(
            "SelectPlayerCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject(
            "SelectPlayerPanel",
            typeof(RectTransform),
            typeof(Image),
            typeof(CharacterSelectionUI));
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.02f, 0.018f, 0.02f, 0.92f);

        CharacterSelectionUI selectionUI = panelObject.GetComponent<CharacterSelectionUI>();
        selectionUI.Configure(controller);
        return selectionUI;
    }

    public void Configure(MenuFlowController controller)
    {
        flowController = controller;
        EnsureInitialized();
        ResetSelection();
    }

    public void SetCharacterOptions(string[] names, Sprite[] portraits)
    {
        if (names != null && names.Length > 0)
        {
            characterNames = names;
        }

        characterPortraits = portraits;
        EnsureInitialized();
        ApplyCharacterOptionContent();
        RefreshSelection();
    }

    public void SelectCharacterIndex(int characterIndex)
    {
        EnsureInitialized();

        int optionCount = OptionCount;
        if (optionCount <= 0)
        {
            return;
        }

        selectedCharacterIndex = Mathf.Clamp(characterIndex, 0, optionCount - 1);
        GameSessionData.SelectedCharacterIndex = selectedCharacterIndex;
        flowController?.SelectCharacterIndex(selectedCharacterIndex);
        RefreshSelection();
    }

    public void StartGame()
    {
        EnsureInitialized();

        if (selectedCharacterIndex < 0)
        {
            RefreshSelection();
            return;
        }

        flowController?.StartGame();
    }

    public void Back()
    {
        EnsureInitialized();

        if (flowController != null)
        {
            flowController.BackToPlayMode();
        }
        else if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
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

        if (buildDefaultLayoutIfEmpty && (characterButtons == null || characterButtons.Length == 0))
        {
            BuildDefaultLayout();
        }

        WireButtons();
        RefreshSelection();
        initialized = true;
    }

    private void ApplyCharacterOptionContent()
    {
        string[] names = GetCharacterNames();

        if (characterLabels != null)
        {
            for (int i = 0; i < characterLabels.Length; i++)
            {
                if (characterLabels[i] != null && i < names.Length)
                {
                    characterLabels[i].text = names[i];
                }
            }
        }

        if (portraitImages == null)
        {
            return;
        }

        for (int i = 0; i < portraitImages.Length; i++)
        {
            Image image = portraitImages[i];
            if (image == null)
            {
                continue;
            }

            Sprite portrait = characterPortraits != null && i < characterPortraits.Length
                ? characterPortraits[i]
                : null;

            image.sprite = portrait;
            image.preserveAspect = portrait != null;
            image.color = portrait != null ? Color.white : GetFallbackPortraitColor(i);
        }
    }

    private void BuildDefaultLayout()
    {
        Image rootImage = rootPanel.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color = new Color(0.02f, 0.018f, 0.02f, 0.92f);
        }

        Font font = GetDefaultFont();
        Transform window = CreateWindow(rootPanel.transform);
        CreateHeader(window, font);

        Transform optionsGrid = CreateOptionsGrid(window);
        string[] names = GetCharacterNames();

        characterButtons = new Button[names.Length];
        characterButtonImages = new Image[names.Length];
        characterLabels = new Text[names.Length];
        portraitImages = new Image[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            CreateCharacterButton(optionsGrid, i, names[i], font);
        }

        statusText = CreateAbsoluteLabel(
            window,
            "SelectionStatus",
            "Pick one player to continue.",
            18,
            FontStyle.Italic,
            TextAnchor.MiddleCenter,
            font,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 145f),
            new Vector2(760f, 40f),
            new Vector2(0.5f, 0f));

        backButton = CreateMapStyleButton(window, "BackBtn", "BACK", font, new Vector2(-358.75f, 53f), Back);
        startButton = CreateMapStyleButton(window, "StartBtn", "START", font, new Vector2(358.75f, 53f), StartGame);
        startButton.interactable = false;
    }

    private void WireButtons()
    {
        if (characterButtons != null)
        {
            for (int i = 0; i < characterButtons.Length; i++)
            {
                int index = i;
                WireButton(characterButtons[i], () => SelectCharacterIndex(index));
            }
        }

        WireButton(startButton, StartGame);
        WireButton(backButton, Back);
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

    private void ResetSelection()
    {
        selectedCharacterIndex = -1;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        bool hasSelection = selectedCharacterIndex >= 0;

        if (startButton != null)
        {
            startButton.interactable = hasSelection;
        }

        string[] names = GetCharacterNames();
        if (statusText != null)
        {
            statusText.text = hasSelection
                ? $"Selected: {names[Mathf.Clamp(selectedCharacterIndex, 0, names.Length - 1)]}"
                : "Pick one player to continue.";
        }

        if (characterButtonImages == null)
        {
            return;
        }

        for (int i = 0; i < characterButtonImages.Length; i++)
        {
            Image image = characterButtonImages[i];
            if (image == null)
            {
                continue;
            }

            image.color = i == selectedCharacterIndex
                ? new Color(0.82f, 0.66f, 0.32f, 1f)
                : new Color(0.16f, 0.18f, 0.22f, 1f);
        }
    }

    private string[] GetCharacterNames()
    {
        return characterNames != null && characterNames.Length > 0
            ? characterNames
            : DefaultCharacterNames;
    }

    private void CreateCharacterButton(Transform parent, int index, string label, Font font)
    {
        GameObject buttonObject = new GameObject(
            $"Player{index + 1}",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(VerticalLayoutGroup));
        buttonObject.transform.SetParent(parent, false);

        Image background = buttonObject.GetComponent<Image>();
        background.color = new Color(0.13f, 0.14f, 0.16f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;

        VerticalLayoutGroup layout = buttonObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 16, 16);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Image portrait = CreatePortrait(buttonObject.transform, index);
        Text text = CreateLabel(buttonObject.transform, label, 20, FontStyle.Bold, TextAnchor.MiddleCenter, font, 44f);

        characterButtons[index] = button;
        characterButtonImages[index] = background;
        characterLabels[index] = text;
        portraitImages[index] = portrait;
    }

    private Image CreatePortrait(Transform parent, int index)
    {
        GameObject portraitObject = new GameObject("Portrait", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        portraitObject.transform.SetParent(parent, false);

        Image image = portraitObject.GetComponent<Image>();
        image.color = GetFallbackPortraitColor(index);

        if (characterPortraits != null && index < characterPortraits.Length && characterPortraits[index] != null)
        {
            image.sprite = characterPortraits[index];
            image.preserveAspect = true;
            image.color = Color.white;
        }

        LayoutElement layoutElement = portraitObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 250f;
        layoutElement.flexibleWidth = 1f;

        return image;
    }

    private Transform CreateWindow(Transform parent)
    {
        GameObject windowObject = new GameObject("Window", typeof(RectTransform), typeof(Image));
        windowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = windowObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Image image = windowObject.GetComponent<Image>();
        image.color = new Color(0.08f, 0.075f, 0.08f, 0.88f);

        return windowObject.transform;
    }

    private void CreateHeader(Transform parent, Font font)
    {
        GameObject headerObject = new GameObject("SelectPlayer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        headerObject.transform.SetParent(parent, false);

        RectTransform rectTransform = headerObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(-39f, -54.29f);
        rectTransform.sizeDelta = new Vector2(1288.902f, 100f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        HorizontalLayoutGroup layout = headerObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        Text title = CreateLabel(headerObject.transform, "SELECT PLAYER", 40, FontStyle.Bold, TextAnchor.MiddleCenter, font, 86f);
        LayoutElement layoutElement = title.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = 656.8f;
    }

    private Transform CreateOptionsGrid(Transform parent)
    {
        GameObject gridObject = new GameObject("Players", typeof(RectTransform), typeof(GridLayoutGroup));
        gridObject.transform.SetParent(parent, false);

        RectTransform rectTransform = gridObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(120f, 210f);
        rectTransform.offsetMax = new Vector2(-120f, -175f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        GridLayoutGroup grid = gridObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(300f, 350f);
        grid.spacing = new Vector2(50f, 50f);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        return gridObject.transform;
    }

    private Button CreateMapStyleButton(Transform parent, string name, string label, Font font, Vector2 anchoredPosition, UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(223.918f, 74.639f);
        rectTransform.pivot = new Vector2(0.5f, 0f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.24f, 0.27f, 0.31f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        Text text = CreateLabel(buttonObject.transform, label, 18, FontStyle.Bold, TextAnchor.MiddleCenter, font, 50f);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private Text CreateAbsoluteLabel(
        Transform parent,
        string name,
        string value,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Font font,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2 pivot)
    {
        GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        rectTransform.pivot = pivot;

        Text text = labelObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        return text;
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
        layoutElement.flexibleWidth = 1f;

        return text;
    }

    private Color GetFallbackPortraitColor(int index)
    {
        switch (index % 4)
        {
            case 0:
                return new Color(0.42f, 0.50f, 0.64f, 1f);
            case 1:
                return new Color(0.46f, 0.58f, 0.44f, 1f);
            case 2:
                return new Color(0.60f, 0.42f, 0.54f, 1f);
            default:
                return new Color(0.62f, 0.50f, 0.38f, 1f);
        }
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
