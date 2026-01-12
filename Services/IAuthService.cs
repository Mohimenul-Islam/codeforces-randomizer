using CodeforcesRandomizer.Models.Auth;

namespace CodeforcesRandomizer.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse?> GetByIdAsync(int userId);
}
