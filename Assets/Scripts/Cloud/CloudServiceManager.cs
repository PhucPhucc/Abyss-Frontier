using UnityEngine;

public class CloudServiceManager : MonoBehaviour
{
    [Header("Chọn backend mode")]
    [SerializeField] private bool useFirebase = false;

    public IAuthService Auth { get; private set; }
    public ICloudSaveService Save { get; private set; }
    public ILeaderboardService Leaderboard { get; private set; }

    private static CloudServiceManager _instance;
    public static CloudServiceManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (useFirebase)
            InitializeFirebase();
        else
            InitializeDummy();
    }

    private void InitializeDummy()
    {
        var dummy = new DummyCloudSave();
        Auth = dummy;
        Save = dummy;
        Leaderboard = dummy;
        Debug.Log("CloudService: Dummy mode (PlayerPrefs)");
    }

    private void InitializeFirebase()
    {
        Debug.LogError("Firebase chưa được import. Đặt useFirebase = false hoặc cài Firebase SDK.");
        InitializeDummy();
    }
}
