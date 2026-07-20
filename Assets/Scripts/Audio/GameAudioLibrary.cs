using UnityEngine;

/// <summary>
/// ScriptableObject lưu trữ danh sách các tài nguyên âm thanh (AudioClip) trong game,
/// bao gồm nhạc nền (BGM) cho các tầng dungeon và hiệu ứng âm thanh (SFX) của Player/Kẻ địch.
/// Được quản lý và gọi bởi AudioManager.
/// </summary>
[CreateAssetMenu(fileName = "GameAudioLibrary", menuName = "Abyss Frontier/Game Audio Library")]
public class GameAudioLibrary : ScriptableObject
{
    [Header("BGM - Nhạc nền")]
    [Tooltip("Nhạc nền dành cho Tầng 1")]
    public AudioClip floor1Music;

    [Tooltip("Nhạc nền dùng chung cho Tầng 2, 3 và 4")]
    public AudioClip floor2To4Music;

    [Tooltip("Nhạc nền màn đấu Boss ở Tầng 5")]
    public AudioClip bossMusic;

    [Header("Player SFX - Hiệu ứng âm thanh người chơi")]
    [Tooltip("Âm thanh khi người chơi thực hiện đòn tấn công")]
    public AudioClip playerAttack;

    [Tooltip("Âm thanh khi người chơi bị trúng đòn / nhận sát thương")]
    public AudioClip playerHurt;

    [Tooltip("Âm thanh khi người chơi bị đánh bại / chết")]
    public AudioClip playerDeath;

    [Header("Enemy SFX - Hiệu ứng âm thanh kẻ địch")]
    [Tooltip("Âm thanh khi kẻ địch thực hiện đòn tấn công")]
    public AudioClip enemyAttack;

    [Tooltip("Âm thanh khi kẻ địch bị trúng đòn / nhận sát thương")]
    public AudioClip enemyHurt;
}
