using UnityEngine;

/// <summary>
/// Sinh các hạt EXP (chấm tròn xanh lá) khi enemy bị tiêu diệt.
/// Tổng EXP chia đều theo giá trị từ EnemyStats.GetExpReward().
/// </summary>
public static class ExpDropSpawner
{
    private const int MaxOrbCount = 18;
    private const float ScatterRadius = 0.4f;
    private const int SpriteResolution = 32;

    private static Sprite circleSprite;

    public static void Spawn(Vector2 position, int totalExp)
    {
        if (totalExp <= 0) return;

        EnsureCircleSprite();

        int orbCount = Mathf.Min(totalExp, MaxOrbCount);
        int baseValue = totalExp / orbCount;
        int remainder = totalExp % orbCount;

        for (int i = 0; i < orbCount; i++)
        {
            int value = baseValue + (i < remainder ? 1 : 0);
            Vector2 offset = Random.insideUnitCircle * ScatterRadius;
            float delay = i * 0.02f;
            ExpOrb.Create(position + offset, value, circleSprite, delay);
        }
    }

    private static void EnsureCircleSprite()
    {
        if (circleSprite != null) return;

        var texture = new Texture2D(SpriteResolution, SpriteResolution, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        float center = (SpriteResolution - 1) * 0.5f;
        float radius = center - 1f;

        for (int y = 0; y < SpriteResolution; y++)
        {
            for (int x = 0; x < SpriteResolution; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = dist <= radius ? 1f : Mathf.Clamp01(1f - (dist - radius));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();

        float pixelsPerUnit = SpriteResolution;
        circleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, SpriteResolution, SpriteResolution),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
    }
}
