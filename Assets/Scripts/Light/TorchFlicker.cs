using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Hiệu ứng ánh sáng lung lay tự nhiên dùng Perlin Noise.
/// Gắn vào Light2D component (TorchLight trên Player hoặc đuốc tĩnh trong Scene).
/// </summary>
[RequireComponent(typeof(Light2D))]
public class TorchFlicker : MonoBehaviour
{
    private Light2D _light;

    [Header("Intensity Flicker")]
    [SerializeField] private float baseIntensity = 1.8f;
    [SerializeField] private float flickerAmount = 0.2f;  // biên độ dao động intensity
    [SerializeField] private float flickerSpeed  = 7f;    // tần số (nhanh = rung nhiều)

    [Header("Radius Flicker")]
    [SerializeField] private float baseRadius    = 4.5f;
    [SerializeField] private float radiusFlicker = 0.3f;

    private float _noiseOffset;

    void Awake()
    {
        _light = GetComponent<Light2D>();
        // Offset ngẫu nhiên: mỗi đuốc flicker độc lập, không đồng bộ nhau
        _noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // Perlin noise: giá trị trong [0, 1], mượt và liên tục
        float noise = Mathf.PerlinNoise(_noiseOffset + Time.time * flickerSpeed, 0f);
        // (noise - 0.5) → [-0.5, 0.5]

        _light.intensity             = baseIntensity + (noise - 0.5f) * flickerAmount * 2f;
        _light.pointLightOuterRadius = baseRadius    + (noise - 0.5f) * radiusFlicker * 2f;
    }
}
