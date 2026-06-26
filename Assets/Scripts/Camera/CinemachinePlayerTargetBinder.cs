using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tự gán Player (theo tag) làm TrackingTarget cho CinemachineCamera.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CinemachineCamera))]
[DefaultExecutionOrder(-50)]
public class CinemachinePlayerTargetBinder : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool assignLookAtTarget;
    [SerializeField] private bool onlyAssignWhenMissing = true;

    private CinemachineCamera cinemachineCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBindersOnSceneCameras()
    {
        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (CinemachineCamera camera in cameras)
        {
            if (camera.GetComponent<CinemachinePlayerTargetBinder>() == null)
                camera.gameObject.AddComponent<CinemachinePlayerTargetBinder>();
        }
    }

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryAssignPlayerTarget();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAssignPlayerTarget();
    }

    private void TryAssignPlayerTarget()
    {
        if (cinemachineCamera == null)
            cinemachineCamera = GetComponent<CinemachineCamera>();

        if (cinemachineCamera == null)
            return;

        if (onlyAssignWhenMissing && cinemachineCamera.Follow != null)
            return;

        Transform playerTransform = FindPlayerTransform();
        if (playerTransform == null)
            return;

        cinemachineCamera.Follow = playerTransform;
        if (assignLookAtTarget)
            cinemachineCamera.LookAt = playerTransform;
    }

    private Transform FindPlayerTransform()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag(playerTag);
        if (taggedPlayer != null)
            return taggedPlayer.transform;

        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        return playerController != null ? playerController.transform : null;
    }
}
