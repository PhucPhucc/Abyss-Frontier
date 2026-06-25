using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Image[] bars;

    public Sprite greenSprite;
    public Sprite brownSprite;

    public Image musicImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private int volumeLevel = 10;
    private bool isMusicOn = true;

    private void Start()
    {
        volumeLevel = PlayerPrefs.GetInt("Volume", 10);
        isMusicOn = PlayerPrefs.GetInt("Music", 1) == 1;

        UpdateUI();
    }

    public void SetVolume(int level)
    {
        volumeLevel = level;

        PlayerPrefs.SetInt("Volume", level);

        if (isMusicOn)
        {
            AudioListener.volume = level / 10f;
        }

        UpdateUI();
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;

        PlayerPrefs.SetInt("Music", isMusicOn ? 1 : 0);

        if (isMusicOn)
        {
            AudioListener.volume = volumeLevel / 10f;
        }
        else
        {
            AudioListener.volume = 0f;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        musicImage.sprite =
            isMusicOn
            ? musicOnSprite
            : musicOffSprite;

        for (int i = 0; i < bars.Length; i++)
        {
            if (!isMusicOn)
            {
                bars[i].sprite = brownSprite;
            }
            else
            {
                bars[i].sprite =
                    i < volumeLevel
                    ? greenSprite
                    : brownSprite;
            }
        }
    }
}