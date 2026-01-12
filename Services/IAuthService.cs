using CodeforcesRandomizer.Models.Auth;

namespace CodeforcesRandomizer.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
}
