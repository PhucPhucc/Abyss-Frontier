using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý hệ thống âm thanh (BGM và SFX) toàn cục trong game.
/// Thiết kế theo dạng Singleton duy nhất và tồn tại xuyên suốt giữa các Scene (DontDestroyOnLoad).
/// </summary>
[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Instance duy nhất của AudioManager (Singleton pattern) để truy cập từ bất kỳ đâu.
    /// </summary>
    public static AudioManager Instance { get; private set; }

    // Đường dẫn mặc định để tự động nạp GameAudioLibrary từ thư mục Assets/Resources/
    private const string LibraryResourcePath = "GameAudioLibrary";

    [Header("Cấu hình & Thư viện âm thanh")]
    [SerializeField, Tooltip("Thư viện chứa các tập tin âm thanh AudioClip của game")]
    private GameAudioLibrary library;

    [SerializeField, Tooltip("AudioSource phát nhạc nền (BGM)")]
    private AudioSource musicSource;

    [SerializeField, Tooltip("AudioSource phát hiệu ứng âm thanh (SFX)")]
    private AudioSource sfxSource;

    [SerializeField, Tooltip("Mức âm lượng nhạc nền (từ 0.0 đến 1.0)")]
    private float musicVolume = 0.7f;

    // Lưu trữ AudioClip nhạc nền đang được phát hiện tại để tránh việc phát lặp lại từ đầu
    private AudioClip currentMusic;

    /// <summary>
    /// Tự động khởi tạo AudioManager nếu chưa có instance nào tồn tại trong Scene sau khi Scene được nạp.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstanceExists()
    {
        if (Instance != null)
            return;

        // Tìm kiếm AudioManager sẵn có trong Scene (kể cả Object đang bị ẩn)
        if (Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include) != null)
            return;

        // Tạo mới GameObject "AudioManager" nếu chưa tồn tại
        var managerObject = new GameObject("AudioManager");
        managerObject.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        // Nạp GameAudioLibrary từ Resources nếu chưa được gán thủ công qua Inspector
        if (library == null)
            library = Resources.Load<GameAudioLibrary>(LibraryResourcePath);

        // Kiểm tra và đảm bảo chỉ giữ lại duy nhất 1 Instance (Singleton)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Giữ GameObject này không bị xóa khi chuyển đổi giữa các Scene
        DontDestroyOnLoad(gameObject);

        // Khởi tạo các thành phần AudioSource nếu chưa có
        EnsureAudioSources();

        // Lắng nghe sự kiện chuyển Scene để phát nhạc nền tương ứng với Scene mới
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Hủy đăng ký lắng nghe sự kiện khi GameObject bị tiêu hủy để tránh rò rỉ bộ nhớ (memory leak)
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Áp dụng cài đặt âm lượng đã lưu từ PlayerPrefs
        ApplySavedVolume();

        // Phát nhạc nền cho Scene đầu tiên khi game khởi chạy
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Đảm bảo các AudioSource phát Music và SFX đã được khởi tạo và cấu hình đúng chuẩn 2D.
    /// </summary>
    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            var musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = true;          // Nhạc nền phát lặp lại liên tục
            musicSource.playOnAwake = false; // Không tự phát khi chưa truyền clip
            musicSource.spatialBlend = 0f;    // Âm thanh 2D (không phụ thuộc khoảng cách/vị trí 3D)
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            var sfxObject = new GameObject("SfxSource");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.loop = false;          // SFX phát một lần rồi dừng
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;    // Âm thanh 2D
        }
    }

    /// <summary>
    /// Đọc và áp dụng cài đặt âm lượng / bật tắt âm thanh được lưu trong PlayerPrefs.
    /// </summary>
    private void ApplySavedVolume()
    {
        int volumeLevel = PlayerPrefs.GetInt("Volume", 10);
        bool isMusicOn = PlayerPrefs.GetInt("Music", 1) == 1;
        // Gán âm lượng tổng của ứng dụng qua AudioListener
        AudioListener.volume = isMusicOn ? volumeLevel / 10f : 0f;
    }

    /// <summary>
    /// Callback được tự động gọi mỗi khi một Scene mới được load xong.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    /// <summary>
    /// Chọn bản nhạc nền tương ứng với tên Scene và thực hiện phát nhạc.
    /// </summary>
    /// <param name="sceneName">Tên của Scene hiện tại (vd: "floor1", "floor2", "floor5")</param>
    private void PlayMusicForScene(string sceneName)
    {
        if (library == null)
        {
            Debug.LogWarning("[AudioManager] GameAudioLibrary is missing.");
            return;
        }

        AudioClip clip = null;
        if (sceneName == "floor1")
            clip = library.floor1Music;
        else if (sceneName is "floor2" or "floor3" or "floor4")
            clip = library.floor2To4Music;
        else if (sceneName is "floor5")
            clip = library.bossMusic;

        PlayMusic(clip);
    }

    /// <summary>
    /// Phát một bản nhạc nền (BGM). Nếu clip trùng với nhạc đang phát thì giữ nguyên.
    /// </summary>
    /// <param name="clip">Tập tin âm thanh AudioClip cần phát</param>
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
            return;

        // Nếu bản nhạc đang phát chính là clip này, không cần phát lại từ đầu
        if (currentMusic == clip && musicSource.isPlaying)
            return;

        currentMusic = clip;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    /// <summary>
    /// Phát một hiệu ứng âm thanh (SFX) một lần duy nhất.
    /// </summary>
    /// <param name="clip">Tập tin âm thanh SFX</param>
    /// <param name="volumeScale">Tỷ lệ âm lượng (mặc định là 1.0)</param>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, volumeScale);
    }

    // Các hàm helper ngắn gọn để phát nhanh hiệu ứng âm thanh cụ thể trong game
    public void PlayPlayerAttack() => PlaySFX(library != null ? library.playerAttack : null);
    public void PlayPlayerHurt() => PlaySFX(library != null ? library.playerHurt : null);
    public void PlayPlayerDeath() => PlaySFX(library != null ? library.playerDeath : null);
    public void PlayEnemyAttack() => PlaySFX(library != null ? library.enemyAttack : null);
    public void PlayEnemyHurt() => PlaySFX(library != null ? library.enemyHurt : null);
}
