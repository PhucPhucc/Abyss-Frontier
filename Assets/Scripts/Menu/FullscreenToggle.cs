using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FullscreenToggle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image checkImage;

    [Header("Sprites")]
    [SerializeField] private Sprite checkedSprite;
    [SerializeField] private Sprite uncheckedSprite;

    private bool isFullscreen;

    private void Start()
    {
        isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        ApplyFullscreen();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.f11Key.wasPressedThisFrame)
        {
            ToggleFullscreen();
        }
    }

    public void ToggleFullscreen()
    {
        isFullscreen = !isFullscreen;

        ApplyFullscreen();

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyFullscreen()
    {
        if (isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }

        checkImage.sprite = isFullscreen ? checkedSprite : uncheckedSprite;
    }
}