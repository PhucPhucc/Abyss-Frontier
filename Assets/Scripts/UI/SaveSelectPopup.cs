using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class SaveSelectPopup : MonoBehaviour
{
    private GameObject popupRoot;
    private Transform buttonContainer;
    private Action<string> onSelect;
    private bool initialized;

    public void Show(List<string> savedMaps, Action<string> onContinue)
    {
        onSelect = onContinue;

        if (initialized)
        {
            RefreshButtons(savedMaps);
            popupRoot.SetActive(true);
            return;
        }

        BuildPopup(savedMaps);
        initialized = true;
        popupRoot.SetActive(true);
    }

    public void Hide()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
        onSelect = null;
    }

    private void OnMapSelected(string sceneName)
    {
        var callback = onSelect;
        Hide();
        callback?.Invoke(sceneName);
    }

    private void RefreshButtons(List<string> savedMaps)
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var map in savedMaps)
            CreateMapButton(map);
    }

    private void BuildPopup(List<string> savedMaps)
    {
        var canvasObj = new GameObject("SaveSelectCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

        var overlayBtn = overlay.GetComponent<Button>();
        overlayBtn.onClick.AddListener(Hide);

        var panelObj = new GameObject("Panel", typeof(Image));
        panelObj.transform.SetParent(canvasObj.transform, false);
        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 300);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        var panelImage = panelObj.GetComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.15f, 1);
        panelImage.raycastTarget = false;

        var textObj = new GameObject("Title", typeof(Text));
        textObj.transform.SetParent(panelObj.transform, false);
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.7f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(20, 0);
        textRect.offsetMax = new Vector2(-20, -10);
        var text = textObj.GetComponent<Text>();
        text.text = "Select Save";
        text.fontSize = 32;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.raycastTarget = false;

        var scrollObj = new GameObject("ScrollView", typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(panelObj.transform, false);
        var scrollRect = scrollObj.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 0.7f);
        scrollRect.offsetMin = new Vector2(10, 10);
        scrollRect.offsetMax = new Vector2(-10, 0);
        var scrollImage = scrollObj.GetComponent<Image>();
        scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 1);

        var viewport = new GameObject("Viewport", typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObj.transform, false);
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.offsetMin = new Vector2(5, 5);
        viewportRect.offsetMax = new Vector2(-5, -5);
        var viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = new Color(0.12f, 0.12f, 0.12f, 1);
        var mask = viewport.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        var content = new GameObject("Content", typeof(VerticalLayoutGroup));
        content.transform.SetParent(viewport.transform, false);
        buttonContainer = content.transform;
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        contentRect.pivot = new Vector2(0.5f, 1);
        var layout = content.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;

        var scrollRectComp = scrollObj.GetComponent<ScrollRect>();
        scrollRectComp.content = contentRect;
        scrollRectComp.viewport = viewportRect;
        scrollRectComp.horizontal = false;
        scrollRectComp.movementType = ScrollRect.MovementType.Clamped;

        foreach (var map in savedMaps)
            CreateMapButton(map);
    }

    private void CreateMapButton(string map)
    {
        var btnObj = new GameObject("Map_" + map, typeof(Image), typeof(Button));
        btnObj.transform.SetParent(buttonContainer, false);
        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(0, 50);

        var image = btnObj.GetComponent<Image>();
        image.color = new Color(0.25f, 0.25f, 0.35f, 1);
        image.raycastTarget = true;

        var btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = image;

        string captured = map;
        btn.onClick.AddListener(() => OnMapSelected(captured));

        var labelObj = new GameObject("Label", typeof(Text));
        labelObj.transform.SetParent(btnObj.transform, false);
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        labelRect.offsetMin = new Vector2(10, 0);
        labelRect.offsetMax = new Vector2(-10, 0);
        var labelText = labelObj.GetComponent<Text>();
        labelText.text = FormatMapName(map);
        labelText.fontSize = 24;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.raycastTarget = false;
    }

    private static string FormatMapName(string sceneName)
    {
        return sceneName.Replace("floor", "Floor ");
    }
}
