using System.Collections;
using UnityEngine;

/// <summary>
/// Hạt EXP rơi ra khi enemy chết. Player chạm vào sẽ hút về và nhận EXP.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class ExpOrb : MonoBehaviour
{
    private static readonly Color OrbColor = new Color(0.25f, 0.95f, 0.35f, 1f);

    [SerializeField] private float magnetRadius = 1.8f;
    [SerializeField] private float magnetSpeed = 6f;
    [SerializeField] private float collectRadius = 0.25f;
    [SerializeField] private float bobAmplitude = 0.04f;
    [SerializeField] private float bobSpeed = 4f;
    [SerializeField] private float lifetime = 45f;

    private int expValue;
    private bool isCollected;
    private bool isSettled;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private Vector3 baseLocalPosition;
    private float bobPhase;

    public static ExpOrb Create(Vector2 position, int value, Sprite circleSprite, float scatterDelay = 0f)
    {
        var go = new GameObject("ExpOrb");
        go.transform.position = position;

        var orb = go.AddComponent<ExpOrb>();
        orb.Initialize(value, circleSprite, scatterDelay);
        return orb;
    }

    private void Initialize(int value, Sprite circleSprite, float scatterDelay)
    {
        expValue = Mathf.Max(1, value);
        bobPhase = Random.Range(0f, Mathf.PI * 2f);

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = circleSprite;
        spriteRenderer.color = OrbColor;
        spriteRenderer.sortingLayerName = "Effects";
        spriteRenderer.sortingOrder = 5;

        float scale = Mathf.Lerp(0.08f, 0.16f, Mathf.InverseLerp(1f, 20f, expValue));
        transform.localScale = Vector3.one * scale;

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        baseLocalPosition = transform.localPosition;
        FindPlayer();
        if (scatterDelay <= 0f)
        {
            isSettled = true;
            baseLocalPosition = transform.position;
        }
        else
        {
            StartCoroutine(ScatterRoutine(scatterDelay));
        }
        Destroy(gameObject, lifetime);
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
                player = pc.gameObject;
        }

        if (player != null)
            playerTransform = player.transform;
    }

    private IEnumerator ScatterRoutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        Vector2 start = transform.position;
        Vector2 end = start + Random.insideUnitCircle * 0.25f;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = 1f - (1f - t) * (1f - t);
            transform.position = Vector2.Lerp(start, end, eased);
            yield return null;
        }

        baseLocalPosition = transform.position;
        isSettled = true;
    }

    private void Update()
    {
        if (isCollected || !isSettled || playerTransform == null)
            return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= collectRadius)
        {
            Collect();
            return;
        }

        if (distance <= magnetRadius)
        {
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            float speed = Mathf.Lerp(magnetSpeed * 0.5f, magnetSpeed * 2f, 1f - distance / magnetRadius);
            transform.position += (Vector3)(dir * speed * Time.deltaTime);
            baseLocalPosition = transform.position;
            return;
        }

        float bob = Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobAmplitude;
        transform.position = baseLocalPosition + Vector3.up * bob;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        if (!other.CompareTag("Player")) return;
        Collect();
    }

    private void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        PlayerStats stats = playerTransform != null
            ? playerTransform.GetComponent<PlayerStats>()
            : null;

        if (stats == null && playerTransform != null)
            stats = playerTransform.GetComponentInParent<PlayerStats>();

        stats?.AddExp(expValue);
        Destroy(gameObject);
    }
}
