using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Màn hình thông báo khi Player chết — hiển thị runtime, không cần setup scene thủ công.
/// </summary>
[DisallowMultipleComponent]
public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private string menuSceneName = "Scene_Menu";
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.35f);

    private PlayerHealth playerHealth;
    private bool built;

    public static void ShowDeath(PlayerHealth player)
    {
        if (player == null)
            return;

        DeathScreenUI screen = FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (screen == null)
            screen = CreateRuntime();

        screen.Show(player);
    }

    public static DeathScreenUI CreateRuntime()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject(
            "Death Screen Canvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject(
            "Death Screen UI",
            typeof(RectTransform),
            typeof(Image),
            typeof(VerticalLayoutGroup),
            typeof(DeathScreenUI));

        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);

        DeathScreenUI screen = panelObject.GetComponent<DeathScreenUI>();
        screen.rootPanel = panelObject;
        screen.EnsureBuilt();
        screen.rootPanel.SetActive(false);
        return screen;
    }

    private void Awake()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void Show(PlayerHealth player)
    {
        playerHealth = player;
        EnsureBuilt();

        if (rootPanel != null)
            rootPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void OnRetryClicked()
    {
        if (playerHealth == null)
            return;

        Time.timeScale = 1f;

        Base_Camp baseCamp = FindFirstObjectByType<Base_Camp>();
        if (baseCamp == null)
        {
            if (rootPanel != null)
                rootPanel.SetActive(false);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        playerHealth.transform.position = (Vector2)baseCamp.transform.position + respawnOffset;
        playerHealth.Respawn();

        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void EnsureBuilt()
    {
        if (built)
            return;

        BuildLayout();
        built = true;
    }

    private void BuildLayout()
    {
        if (rootPanel == null)
            rootPanel = gameObject;

        if (rootPanel.transform.childCount > 0)
            return;

        VerticalLayoutGroup layout = rootPanel.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
            layout = rootPanel.AddComponent<VerticalLayoutGroup>();

        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        CreateLabel(rootPanel.transform, "You Die", 56, FontStyle.Bold, font, 90f);
        CreateButton(rootPanel.transform, "Choi lai", font, OnRetryClicked);
        CreateButton(rootPanel.transform, "Ve Menu", font, OnMainMenuClicked);
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

        if (inputModule.actionsAsset == null)
            inputModule.AssignDefaultActions();
    }

    private static Text CreateLabel(Transform parent, string value, int fontSize, FontStyle fontStyle, Font font, float height)
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        labelObject.transform.SetParent(parent, false);

        Text text = labelObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.95f, 0.3f, 0.3f, 1f);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        LayoutElement layoutElement = labelObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = height;
        layoutElement.preferredWidth = 520f;

        return text;
    }

    private static void CreateButton(Transform parent, string label, Font font, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.16f, 0.18f, 0.22f, 1f);

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 52f;
        layoutElement.preferredWidth = 320f;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);

        Text text = textObject.GetComponent<Text>();
        text.text = label;
        text.font = font;
        text.fontSize = 22;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
