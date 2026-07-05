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
    [SerializeField] private float baseIntensity = 1.8f; // Cường độ sáng cơ bản
    [SerializeField] private float flickerAmount = 0.2f; // Biên độ dao động cường độ
    [SerializeField] private float flickerSpeed  = 0.5f;   // Tần số flicker (càng cao càng rung nhanh)

    [Header("Radius Flicker")]
    [SerializeField] private float baseRadius    = 2f; // Bán kính sáng cơ bản
    [SerializeField] private float radiusFlicker = 0.3f; // Biên độ dao động bán kính

    private float _noiseOffset; // Offset ngẫu nhiên để mỗi đuốc flicker độc lập

    void Awake()
    {
        _light = GetComponent<Light2D>();
        // Offset ngẫu nhiên giúp các đuốc không flicker đồng bộ với nhau
        _noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // Perlin noise tạo giá trị mượt, liên tục trong [0, 1]
        float noise = Mathf.PerlinNoise(_noiseOffset + Time.time * flickerSpeed, 0f);

        // Chuyển noise từ [0,1] sang [-0.5, 0.5] rồi nhân với biên độ
        _light.intensity             = baseIntensity + (noise - 0.5f) * flickerAmount * 2f;
        _light.pointLightOuterRadius = baseRadius    + (noise - 0.5f) * radiusFlicker * 2f;
    }
}
