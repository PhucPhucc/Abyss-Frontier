using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string LibraryResourcePath = "GameAudioLibrary";

    [SerializeField] private GameAudioLibrary library;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float musicVolume = 0.7f;

    private AudioClip currentMusic;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstanceExists()
    {
        if (Instance != null)
            return;

        if (Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include) != null)
            return;

        var managerObject = new GameObject("AudioManager");
        managerObject.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (library == null)
            library = Resources.Load<GameAudioLibrary>(LibraryResourcePath);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplySavedVolume();
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            var musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            var sfxObject = new GameObject("SfxSource");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    private void ApplySavedVolume()
    {
        int volumeLevel = PlayerPrefs.GetInt("Volume", 10);
        bool isMusicOn = PlayerPrefs.GetInt("Music", 1) == 1;
        AudioListener.volume = isMusicOn ? volumeLevel / 10f : 0f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

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
        else if (sceneName is "floor5" or "floor6")
            clip = library.bossMusic;

        PlayMusic(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
            return;

        if (currentMusic == clip && musicSource.isPlaying)
            return;

        currentMusic = clip;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, volumeScale);
    }

    public void PlayPlayerAttack() => PlaySFX(library != null ? library.playerAttack : null);
    public void PlayPlayerHurt() => PlaySFX(library != null ? library.playerHurt : null);
    public void PlayPlayerDeath() => PlaySFX(library != null ? library.playerDeath : null);
    public void PlayEnemyAttack() => PlaySFX(library != null ? library.enemyAttack : null);
    public void PlayEnemyHurt() => PlaySFX(library != null ? library.enemyHurt : null);
}
