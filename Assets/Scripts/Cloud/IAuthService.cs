using System.Threading.Tasks;

public interface IAuthService
{
    Task<AuthResult> LoginAnonymously();
    Task<AuthResult> LoginWithEmail(string email, string password);
    Task<AuthResult> RegisterWithEmail(string email, string password);
    void Logout();
    bool IsLoggedIn { get; }
    string UserId { get; }
    string DisplayName { get; }
}

public struct AuthResult
{
    public bool Success;
    public string ErrorMessage;
    public string UserId;
    public string DisplayName;
}
