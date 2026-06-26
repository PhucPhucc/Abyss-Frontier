using UnityEngine;

[CreateAssetMenu(fileName = "GameAudioLibrary", menuName = "Abyss Frontier/Game Audio Library")]
public class GameAudioLibrary : ScriptableObject
{
    [Header("BGM")]
    public AudioClip floor1Music;
    public AudioClip floor2To4Music;
    public AudioClip bossMusic;

    [Header("Player SFX")]
    public AudioClip playerAttack;
    public AudioClip playerHurt;
    public AudioClip playerDeath;

    [Header("Enemy SFX")]
    public AudioClip enemyAttack;
    public AudioClip enemyHurt;
}
