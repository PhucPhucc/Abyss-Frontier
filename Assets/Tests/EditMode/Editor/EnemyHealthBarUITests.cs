using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUITests
{
    [Test]
    public void BuildsWorldSpaceBarAboveTallSpriteWithUiSorting()
    {
        GameObject enemy = new GameObject("TallEnemy");
        Texture2D texture = null;
        Sprite sprite = null;

        try
        {
            texture = new Texture2D(32, 64, TextureFormat.RGBA32, false);
            sprite = Sprite.Create(texture, new Rect(0f, 0f, 32f, 64f), new Vector2(0.5f, 0.5f), 32f);

            SpriteRenderer spriteRenderer = enemy.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;

            EnemyHealth health = enemy.AddComponent<EnemyHealth>();
            EnemyHealthBarUI healthBar = enemy.AddComponent<EnemyHealthBarUI>();
            healthBar.SetTarget(health);

            Transform canvasTransform = enemy.transform.Find("Enemy HP Canvas");
            Assert.IsNotNull(canvasTransform, "Expected runtime Enemy HP Canvas child.");

            Canvas canvas = canvasTransform.GetComponent<Canvas>();
            Assert.IsNotNull(canvas);
            Assert.AreEqual(RenderMode.WorldSpace, canvas.renderMode);
            Assert.AreEqual("UI", canvas.sortingLayerName);
            Assert.AreEqual(100, canvas.sortingOrder);
            Assert.IsTrue(canvas.overrideSorting);

            // Tall sprite (2 units high, centered pivot) → top at y=1 + 0.2 padding
            Assert.AreEqual(1.2f, canvasTransform.localPosition.y, 0.001f);

            Image[] images = canvasTransform.GetComponentsInChildren<Image>(true);
            Assert.AreEqual(2, images.Length);
            foreach (Image image in images)
            {
                Assert.IsNotNull(image.sprite, "Health bar Images need a white sprite to render.");
            }
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            if (sprite != null)
            {
                Object.DestroyImmediate(sprite);
            }

            if (texture != null)
            {
                Object.DestroyImmediate(texture);
            }
        }
    }

    [Test]
    public void FallsBackToDefaultOffsetWhenSpriteMissing()
    {
        GameObject enemy = new GameObject("SpritelessEnemy");

        try
        {
            EnemyHealth health = enemy.AddComponent<EnemyHealth>();
            EnemyHealthBarUI healthBar = enemy.AddComponent<EnemyHealthBarUI>();
            healthBar.SetTarget(health);

            Transform canvasTransform = enemy.transform.Find("Enemy HP Canvas");
            Assert.IsNotNull(canvasTransform);
            Assert.AreEqual(1.05f, canvasTransform.localPosition.y, 0.001f);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
        }
    }
}
