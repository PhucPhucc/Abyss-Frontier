using System.Collections;
using UnityEngine;

/// <summary>
/// Ghost trail khi dash — clone sprite hiện tại rồi mờ dần.
/// </summary>
public static class DashAfterimage
{
    private const float FadeDuration = 0.22f;
    private const float StartAlpha = 0.55f;

    public static void SpawnFromCharacter(Transform characterRoot)
    {
        if (characterRoot == null) return;

        foreach (SpriteRenderer source in characterRoot.GetComponentsInChildren<SpriteRenderer>())
        {
            if (!source.enabled || source.sprite == null)
                continue;

            SpawnSingle(source);
        }
    }

    private static void SpawnSingle(SpriteRenderer source)
    {
        var ghost = new GameObject("DashAfterimage");
        ghost.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        ghost.transform.localScale = source.transform.lossyScale;

        var ghostRenderer = ghost.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = source.sprite;
        ghostRenderer.flipX = source.flipX;
        ghostRenderer.flipY = source.flipY;
        ghostRenderer.sortingLayerID = source.sortingLayerID;
        ghostRenderer.sortingOrder = source.sortingOrder - 1;
        ghostRenderer.color = new Color(0.75f, 0.95f, 1f, StartAlpha);

        ghost.AddComponent<DashAfterimageFade>();
    }

    private class DashAfterimageFade : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private float elapsed;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = elapsed / FadeDuration;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(StartAlpha, 0f, t);
            spriteRenderer.color = color;
        }
    }
}
