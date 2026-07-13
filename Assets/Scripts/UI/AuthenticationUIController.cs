using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[DisallowMultipleComponent]
public class AuthenticationUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject signUpPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button menuLoginBtn;
    [SerializeField] private Button menuSignUpBtn;

    [Header("Panel Navigation Buttons")]
    [SerializeField] private Button switchToSignUpBtn; // Nút chuyển sang SignUpPanel trong LoginPanel
    [SerializeField] private Button switchToLoginBtn;  // Nút chuyển sang LoginPanel trong SignUpPanel
    [SerializeField] private Button loginBackBtn;      // Nút quay lại Menu từ LoginPanel
    [SerializeField] private Button signUpBackBtn;     // Nút quay lại Menu từ SignUpPanel

    [Header("Login Form (Optional)")]
    [SerializeField] private TMP_InputField loginEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Button loginPasswordToggleBtn;
    [SerializeField] private Button loginSubmitBtn;
    [SerializeField] private TMP_Text loginErrorText;

    [Header("SignUp Form (Optional)")]
    [SerializeField] private TMP_InputField signUpEmailInput;
    [SerializeField] private TMP_InputField signUpPasswordInput;
    [SerializeField] private TMP_InputField signUpConfirmInput;
    [SerializeField] private Button signUpPasswordToggleBtn;
    [SerializeField] private Button signUpConfirmToggleBtn;
    [SerializeField] private Button signUpSubmitBtn;
    [SerializeField] private TMP_Text signUpErrorText;

    [Header("Settings")]
    [SerializeField] private string nextSceneName = "Scene_Menu";

    private void Awake()
    {
        // Gán sự kiện chuyển đổi trạng thái Panel
        if (menuLoginBtn != null)
            menuLoginBtn.onClick.AddListener(ShowLoginPanel);

        if (menuSignUpBtn != null)
            menuSignUpBtn.onClick.AddListener(ShowSignUpPanel);

        if (switchToSignUpBtn != null)
            switchToSignUpBtn.onClick.AddListener(ShowSignUpPanel);

        if (switchToLoginBtn != null)
            switchToLoginBtn.onClick.AddListener(ShowLoginPanel);

        if (loginBackBtn != null)
            loginBackBtn.onClick.AddListener(ShowMenuPanel);

        if (signUpBackBtn != null)
            signUpBackBtn.onClick.AddListener(ShowMenuPanel);

        // Gán sự kiện submit form đăng nhập / đăng ký
        if (loginSubmitBtn != null)
            loginSubmitBtn.onClick.AddListener(OnLoginClicked);

        if (signUpSubmitBtn != null)
            signUpSubmitBtn.onClick.AddListener(OnSignUpClicked);

        // Toggle show/hide password
        if (loginPasswordToggleBtn != null && loginPasswordInput != null)
            loginPasswordToggleBtn.onClick.AddListener(() => TogglePasswordVisibility(loginPasswordInput, loginPasswordToggleBtn));

        if (signUpPasswordToggleBtn != null && signUpPasswordInput != null)
            signUpPasswordToggleBtn.onClick.AddListener(() => TogglePasswordVisibility(signUpPasswordInput, signUpPasswordToggleBtn));

        if (signUpConfirmToggleBtn != null && signUpConfirmInput != null)
            signUpConfirmToggleBtn.onClick.AddListener(() => TogglePasswordVisibility(signUpConfirmInput, signUpConfirmToggleBtn));
    }

    private void Start()
    {
        ConfigureEmailFields();
        EnsurePasswordFields();
        ShowMenuPanel();
    }

    private void ConfigureEmailFields()
    {
        if (signUpEmailInput != null)
        {
            signUpEmailInput.contentType = TMP_InputField.ContentType.Standard;
            signUpEmailInput.onValidateInput = (string text, int idx, char ch) => ch == '"' ? '@' : ch;
            signUpEmailInput.ForceLabelUpdate();
        }
        if (loginEmailInput != null)
        {
            loginEmailInput.contentType = TMP_InputField.ContentType.Standard;
            loginEmailInput.onValidateInput = (string text, int idx, char ch) => ch == '"' ? '@' : ch;
            loginEmailInput.ForceLabelUpdate();
        }
    }

    private void EnsurePasswordFields()
    {
        SetPasswordContentType(signUpPasswordInput);
        SetPasswordContentType(signUpConfirmInput);
        SetPasswordContentType(loginPasswordInput);
    }

    private void SetPasswordContentType(TMP_InputField input)
    {
        if (input == null) return;
        input.contentType = TMP_InputField.ContentType.Password;
        input.ForceLabelUpdate();
    }

    private void TogglePasswordVisibility(TMP_InputField input, Button toggleBtn)
    {
        if (input == null) return;

        bool isHidden = input.contentType == TMP_InputField.ContentType.Password;
        input.contentType = isHidden ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        input.ForceLabelUpdate();

        if (toggleBtn != null)
        {
            var label = toggleBtn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = isHidden ? "●" : "○";
        }
    }

    public void ShowMenuPanel()
    {
        SetPanels(showMenu: true, showLogin: false, showSignUp: false);
        ClearErrors();
    }

    public void ShowLoginPanel()
    {
        SetPanels(showMenu: false, showLogin: true, showSignUp: false);
        ClearErrors();
    }

    public void ShowSignUpPanel()
    {
        SetPanels(showMenu: false, showLogin: false, showSignUp: true);
        ClearErrors();
    }

    private void SetPanels(bool showMenu, bool showLogin, bool showSignUp)
    {
        if (menuPanel != null) menuPanel.SetActive(showMenu);
        if (loginPanel != null) loginPanel.SetActive(showLogin);
        if (signUpPanel != null) signUpPanel.SetActive(showSignUp);
    }

    private void ClearErrors()
    {
        if (loginErrorText != null) loginErrorText.text = "";
        if (signUpErrorText != null) signUpErrorText.text = "";
    }

    public async void OnLoginClicked()
    {
        ClearErrors();

        if (loginEmailInput == null || loginPasswordInput == null)
        {
            Debug.LogError($"[AuthUI] Login inputs not assigned! email={loginEmailInput}, password={loginPasswordInput}");
            if (loginErrorText != null) loginErrorText.text = "Lỗi cấu hình form đăng nhập!";
            return;
        }

        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;
        Debug.Log($"[AuthUI] Login attempt: email='{email}' (len={email.Length}), password length={password.Length}");

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            if (loginErrorText != null) loginErrorText.text = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
            return;
        }

        if (CloudServiceManager.Instance != null && CloudServiceManager.Instance.Auth != null)
        {
            if (loginSubmitBtn != null) loginSubmitBtn.interactable = false;

            var result = await CloudServiceManager.Instance.Auth.LoginWithEmail(email, password);

            if (loginSubmitBtn != null)
            {
                loginSubmitBtn.interactable = true;
                Debug.Log("[AuthUI] loginSubmitBtn set to interactable = true");
            }

            if (result.Success)
            {
                SaveManager.LoggedInEmail = email;
                LoadNextScene();
            }
            else
            {
                if (loginErrorText != null)
                    loginErrorText.text = $"Đăng nhập thất bại: {result.ErrorMessage}";
                else
                    Debug.LogWarning("[AuthUI] loginErrorText chưa được gán trong Inspector!");
            }
        }
        else
        {
            Debug.Log($"[Auth] Đăng nhập thành công (Offline): {email}");
            SaveManager.LoggedInEmail = email;
            LoadNextScene();
        }
    }

    public async void OnSignUpClicked()
    {
        ClearErrors();

        string email = signUpEmailInput != null ? signUpEmailInput.text.Trim() : "";
        string password = signUpPasswordInput != null ? signUpPasswordInput.text : "";
        string confirm = signUpConfirmInput != null ? signUpConfirmInput.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            if (signUpErrorText != null) signUpErrorText.text = "Vui lòng nhập đầy đủ thông tin!";
            return;
        }

        if (signUpConfirmInput != null && password != confirm)
        {
            if (signUpErrorText != null) signUpErrorText.text = "Mật khẩu xác nhận không khớp!";
            return;
        }

        if (CloudServiceManager.Instance != null && CloudServiceManager.Instance.Auth != null)
        {
            if (signUpSubmitBtn != null) signUpSubmitBtn.interactable = false;

            var result = await CloudServiceManager.Instance.Auth.RegisterWithEmail(email, password);

            if (signUpSubmitBtn != null)
            {
                signUpSubmitBtn.interactable = true;
                Debug.Log("[AuthUI] signUpSubmitBtn set to interactable = true");
            }

            if (result.Success)
            {
                SaveManager.LoggedInEmail = email;
                LoadNextScene();
            }
            else
            {
                if (signUpErrorText != null)
                    signUpErrorText.text = $"Đăng ký thất bại: {result.ErrorMessage}";
                else
                    Debug.LogWarning("[AuthUI] signUpErrorText chưa được gán trong Inspector!");
            }
        }
        else
        {
            Debug.Log($"[Auth] Đăng ký thành công (Offline): {email}");
            SaveManager.LoggedInEmail = email;
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
