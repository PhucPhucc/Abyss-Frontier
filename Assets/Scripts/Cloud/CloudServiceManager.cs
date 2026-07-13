using UnityEngine;

public class CloudServiceManager : MonoBehaviour
{
    [Header("Chọn backend mode")]
    [SerializeField] private bool useFirebase = true;

    public IAuthService Auth { get; private set; }
    public ICloudSaveService Save { get; private set; }
    public ILeaderboardService Leaderboard { get; private set; }

    public bool IsAuthReady { get; private set; }
    public event System.Action AuthReady;

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

    private async void Start()
    {
        if (Auth == null) return;

        if (Auth is FirebaseCloudSave fb)
        {
            await fb.WaitForInit();
            if (!fb.IsReady)
            {
                Debug.LogWarning("Firebase init failed, fallback to Dummy mode");
                InitializeDummy();
            }
        }

        AuthResult result;
        try
        {
            result = await Auth.LoginAnonymously();
        }
        catch (System.Exception e)
        {
            Debug.Log($"Auth exception (normal if Firebase not configured): {e.Message}");
            result = new AuthResult { Success = false, ErrorMessage = e.Message };
        }

        if (!result.Success && Auth is FirebaseCloudSave)
        {
            Debug.LogWarning("Firebase login failed, fallback to Dummy mode");
            InitializeDummy();
            result = await Auth.LoginAnonymously();
        }

        IsAuthReady = result.Success;
        if (result.Success)
        {
            Debug.Log($"Auth OK: {result.UserId}");
            AuthReady?.Invoke();
        }
        else
            Debug.LogError($"Auth failed: {result.ErrorMessage}");
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
#if FB_SDK
        var fb = new FirebaseCloudSave();
        fb.Initialize();
        Auth = fb;
        Save = fb;
        Leaderboard = fb;
        Debug.Log("CloudService: Firebase mode");
#else
        Debug.LogError("FB_SDK chưa được define. Đặt useFirebase = false hoặc thêm FB_SDK vào Scripting Define Symbols.");
        InitializeDummy();
#endif
    }
}
