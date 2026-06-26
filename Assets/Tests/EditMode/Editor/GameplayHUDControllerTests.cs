using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHUDControllerTests
{
    [Test]
    public void HudHealthBarGeometryFollowsPlayerHealth()
    {
        GameObject player = new GameObject("Player");
        GameplayHUDController hud = GameplayHUDController.CreateRuntimeHud();

        try
        {
            PlayerStats stats = player.AddComponent<PlayerStats>();
            PlayerHealth health = player.AddComponent<PlayerHealth>();

            hud.SetPlayer(stats);
            health.TakeDamage(35);
            hud.Refresh();

            Image hpFill = GetPrivateImage(hud, "hpFill");

            Assert.NotNull(hpFill);
            Assert.AreEqual(0.5f, hpFill.rectTransform.anchorMax.x, 0.001f);
        }
        finally
        {
            Object.DestroyImmediate(hud.gameObject);
            Object.DestroyImmediate(player);
        }
    }

    private static Image GetPrivateImage(GameplayHUDController hud, string fieldName)
    {
        FieldInfo field = typeof(GameplayHUDController).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(hud) as Image;
    }
}
