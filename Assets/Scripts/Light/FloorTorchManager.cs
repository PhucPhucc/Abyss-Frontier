using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using System.Collections;

public class FloorTorchManager : MonoBehaviour
{
    [Header("Tag của nhân vật cần tìm")]
    [SerializeField] private string playerTag = "Player";

    [Header("Cấu hình Light2D gắn vào Player")]
    [SerializeField] private float lightIntensity = 1.8f;
    [SerializeField] private float lightOuterRadius = 2f;
    [SerializeField] private Color lightColor = Color.white;

    [Header("Cấu hình TorchFlicker")]
    [SerializeField] private float baseIntensity = 1.8f;
    [SerializeField] private float flickerAmount = 0.2f;
    [SerializeField] private float flickerSpeed = 0.5f;
    [SerializeField] private float baseRadius = 2f;
    [SerializeField] private float radiusFlicker = 0.3f;

    private void Start()
    {
        StartCoroutine(CheckAndAttachTorchRoutine());
    }

    private IEnumerator CheckAndAttachTorchRoutine()
    {
        while (true)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

            foreach (GameObject player in players)
            {
                //----------------------------------
                // Light2D
                //----------------------------------
                Light2D light2D = player.GetComponent<Light2D>();

                if (light2D == null)
                {
                    light2D = player.AddComponent<Light2D>();

                    light2D.lightType = Light2D.LightType.Point;
                    light2D.intensity = lightIntensity;
                    light2D.pointLightOuterRadius = lightOuterRadius;
                    light2D.color = lightColor;
                }

                //----------------------------------
                // TorchFlicker
                //----------------------------------
                TorchFlicker flicker = player.GetComponent<TorchFlicker>();

                if (flicker == null)
                {
                    flicker = player.AddComponent<TorchFlicker>();
                    flicker.Init(baseIntensity, flickerAmount, flickerSpeed, baseRadius, radiusFlicker);
                }

                //----------------------------------
                // PlayerTorchInteraction
                //----------------------------------
                PlayerTorchInteraction interaction = player.GetComponent<PlayerTorchInteraction>();

                if (interaction == null)
                {
                    player.AddComponent<PlayerTorchInteraction>();
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}