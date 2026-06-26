using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class StatScreenUITests
{
    [Test]
    public void RuntimeStatScreenUsesWideTwoColumnLayout()
    {
        StatScreenUI screen = StatScreenUI.CreateRuntimeScreen();

        try
        {
            RectTransform rectTransform = screen.GetComponent<RectTransform>();
            Assert.AreEqual(new Vector2(0.1f, 0.1f), rectTransform.anchorMin);
            Assert.AreEqual(new Vector2(0.9f, 0.9f), rectTransform.anchorMax);

            Transform content = screen.transform.Find("Stat Content");
            Assert.NotNull(content);
            Assert.NotNull(content.GetComponent<HorizontalLayoutGroup>());
            Assert.NotNull(content.Find("Stats Left Column"));
            Assert.NotNull(content.Find("Buttons Right Column"));
        }
        finally
        {
            Object.DestroyImmediate(screen.transform.root.gameObject);
        }
    }

    [Test]
    public void RuntimeStatScreenCreatesClickableEventSystem()
    {
        GameObject existingEventSystem = EventSystem.current != null ? EventSystem.current.gameObject : null;
        StatScreenUI screen = StatScreenUI.CreateRuntimeScreen();

        try
        {
            Assert.NotNull(EventSystem.current);
            Assert.NotNull(EventSystem.current.GetComponent<InputSystemUIInputModule>());
        }
        finally
        {
            Object.DestroyImmediate(screen.transform.root.gameObject);

            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem != null && currentEventSystem.gameObject != existingEventSystem)
            {
                Object.DestroyImmediate(currentEventSystem.gameObject);
            }
        }
    }

    [Test]
    public void OpeningAssignedStatScreenCreatesClickableEventSystem()
    {
        GameObject existingEventSystem = EventSystem.current != null ? EventSystem.current.gameObject : null;
        GameObject player = new GameObject("Player");
        GameObject canvas = new GameObject("Assigned Stat Canvas", typeof(Canvas), typeof(GraphicRaycaster));
        GameObject panel = new GameObject("Assigned Stat Panel", typeof(RectTransform), typeof(StatScreenUI));
        panel.transform.SetParent(canvas.transform, false);

        try
        {
            PlayerStats stats = player.AddComponent<PlayerStats>();
            StatScreenUI screen = panel.GetComponent<StatScreenUI>();

            screen.Open(stats);

            Assert.NotNull(EventSystem.current);
            Assert.NotNull(EventSystem.current.GetComponent<InputSystemUIInputModule>());
        }
        finally
        {
            Object.DestroyImmediate(canvas);
            Object.DestroyImmediate(player);

            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem != null && currentEventSystem.gameObject != existingEventSystem)
            {
                Object.DestroyImmediate(currentEventSystem.gameObject);
            }
        }
    }
}
