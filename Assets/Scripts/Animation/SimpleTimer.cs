using TMPro;
using UnityEngine;

public class SimpleTimer : MonoBehaviour
{
    [Header("Count down time (seconds)")]
    [SerializeField] private float maxTime = 300f;

    [Header("Countdown text")]
    [SerializeField] private TextMeshProUGUI timeText;

    private float currentTime;
    private bool hasExpired;
    private PlayerHealth playerHealth;

    private void OnEnable()
    {
        BindPlayer();
    }

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        BindPlayer();

        if (hasExpired)
            return;

        currentTime = Mathf.Max(0f, currentTime - Time.deltaTime);
        UpdateTimeDisplay();

        if (currentTime <= 0f)
            TimeIsUp();
    }

    private void OnDisable()
    {
        UnbindPlayer();
    }

    public void ResetTimer()
    {
        currentTime = Mathf.Max(0f, maxTime);
        hasExpired = false;
        UpdateTimeDisplay();
    }

    private void TimeIsUp()
    {
        currentTime = 0f;
        UpdateTimeDisplay();

        if (hasExpired)
            return;

        BindPlayer();
        if (playerHealth == null)
            return;

        hasExpired = true;
        Debug.Log("Timer expired. Triggering game over.");
        playerHealth.TriggerGameOver();
    }

    private void BindPlayer()
    {
        PlayerHealth foundPlayer = FindFirstObjectByType<PlayerHealth>();
        if (foundPlayer == playerHealth)
            return;

        UnbindPlayer();
        playerHealth = foundPlayer;

        if (playerHealth != null)
            playerHealth.Respawned += ResetTimer;
    }

    private void UnbindPlayer()
    {
        if (playerHealth != null)
            playerHealth.Respawned -= ResetTimer;

        playerHealth = null;
    }

    private void UpdateTimeDisplay()
    {
        if (timeText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(currentTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timeText.text = $"{minutes:00}:{seconds:00}";
    }
}
