using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableLever : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DoorController linkedDoor;
    [SerializeField] private Animator leverAnimator;

    [Header("Interaction")]
    [SerializeField] private float interactRange = 1.5f;

    private bool _isActivated = false;
    private Transform _playerTransform;

    private void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (_isActivated) return;
        if (_playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, _playerTransform.position);

        if (dist <= interactRange &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Activate();
        }
    }

    public void Activate()
    {
        if (_isActivated) return;

        _isActivated = true;
        leverAnimator?.SetTrigger("Pull");
        linkedDoor?.OpenDoor();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}