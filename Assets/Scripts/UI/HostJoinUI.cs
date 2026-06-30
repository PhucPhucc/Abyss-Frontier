using UnityEngine;
using UnityEngine.UI;

public class HostJoinUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject hostJoinPanel;
    [SerializeField] private InputField sessionNameInput;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;

    [Header("Menu Flow")]
    [SerializeField] private MenuFlowController menuFlow;

    private void Start()
    {
        if (hostButton != null) hostButton.onClick.AddListener(OnHostClicked);
        if (joinButton != null) joinButton.onClick.AddListener(OnJoinClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

        if (hostJoinPanel != null)
            hostJoinPanel.SetActive(false);
    }

    public void Show()
    {
        if (hostJoinPanel != null)
            hostJoinPanel.SetActive(true);

        if (sessionNameInput != null)
            sessionNameInput.text = GameSessionData.SessionName;
    }

    public void Hide()
    {
        if (hostJoinPanel != null)
            hostJoinPanel.SetActive(false);
    }

    private void OnHostClicked()
    {
        if (sessionNameInput != null && !string.IsNullOrEmpty(sessionNameInput.text))
            GameSessionData.SessionName = sessionNameInput.text;

        GameSessionData.IsMultiplayer = true;
        GameSessionData.IsHost = true;

        if (menuFlow != null)
            menuFlow.OnHostSelected();

        Hide();
    }

    private void OnJoinClicked()
    {
        if (sessionNameInput != null && !string.IsNullOrEmpty(sessionNameInput.text))
            GameSessionData.SessionName = sessionNameInput.text;

        GameSessionData.IsMultiplayer = true;
        GameSessionData.IsHost = false;

        if (menuFlow != null)
            menuFlow.OnJoinSelected();

        Hide();
    }

    private void OnBackClicked()
    {
        if (menuFlow != null)
            menuFlow.OnHostJoinBack();

        Hide();
    }
}
