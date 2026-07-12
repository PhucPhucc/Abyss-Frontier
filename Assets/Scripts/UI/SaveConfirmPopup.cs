using UnityEngine;
using UnityEngine.UI;
using System;

public class SaveConfirmPopup : MonoBehaviour
{
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action onYes;
    private Action onNo;
    private bool initialized;

    private void Awake()
    {
        if (popupRoot != null && yesButton != null && noButton != null)
        {
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(OnNoClicked);
            initialized = true;
        }
    }

    public void Show(Action onSave, Action onSkip)
    {
        onYes = onSave;
        onNo = onSkip;

        if (initialized)
        {
            popupRoot.SetActive(true);
            return;
        }

        BuildPopup();
        initialized = true;
        popupRoot.SetActive(true);
    }

    public void Hide()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
        onYes = null;
        onNo = null;
    }

    private void OnYesClicked()
    {
        var callback = onYes;
        Hide();
        callback?.Invoke();
    }

    private void OnNoClicked()
    {
        var callback = onNo;
        Hide();
        callback?.Invoke();
    }

    private void BuildPopup()
    {
        var canvasObj = new GameObject("SaveConfirmCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        popupRoot = canvasObj;

        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var overlay = new GameObject("Overlay", typeof(Image), typeof(Button));
        overlay.transform.SetParent(canvasObj.transform, false);
        var overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        var overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.6f);
        overlayImage.raycastTarget = true;

        var panelObj = new GameObject("Panel", typeof(Image));
        panelObj.transform.SetParent(canvasObj.transform, false);
        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 220);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        var panelImage = panelObj.GetComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.15f, 1);
        panelImage.raycastTarget = false;

        var textObj = new GameObject("Message", typeof(Text));
        textObj.transform.SetParent(panelObj.transform, false);
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(20, 0);
        textRect.offsetMax = new Vector2(-20, -10);
        var text = textObj.GetComponent<Text>();
        text.text = "Save game?";
        text.fontSize = 32;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.raycastTarget = false;

        var btnGroup = new GameObject("Buttons", typeof(HorizontalLayoutGroup));
        btnGroup.transform.SetParent(panelObj.transform, false);
        var btnGroupRect = btnGroup.GetComponent<RectTransform>();
        btnGroupRect.anchorMin = new Vector2(0, 0);
        btnGroupRect.anchorMax = new Vector2(1, 0.5f);
        btnGroupRect.offsetMin = new Vector2(20, 10);
        btnGroupRect.offsetMax = new Vector2(-20, 0);
        var layout = btnGroup.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 30;
        layout.childAlignment = TextAnchor.MiddleCenter;

        yesButton = CreateButton(btnGroup.transform, "YesButton", "  Yes  ", new Color(0.2f, 0.5f, 0.2f));
        yesButton.onClick.AddListener(OnYesClicked);

        noButton = CreateButton(btnGroup.transform, "NoButton", "  No  ", new Color(0.5f, 0.2f, 0.2f));
        noButton.onClick.AddListener(OnNoClicked);
    }

    private static Button CreateButton(Transform parent, string name, string label, Color color)
    {
        var btnObj = new GameObject(name, typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(120, 50);

        var image = btnObj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;

        var btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = image;

        var labelObj = new GameObject("Label", typeof(Text));
        labelObj.transform.SetParent(btnObj.transform, false);
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        var labelText = labelObj.GetComponent<Text>();
        labelText.text = label;
        labelText.fontSize = 28;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.raycastTarget = false;

        return btn;
    }
}
