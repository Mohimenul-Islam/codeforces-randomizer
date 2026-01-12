using System.Net;
using CodeforcesRandomizer.Data;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeforcesRandomizer.Services;

public class AuthService(AppDbContext dbContext, HttpClient httpClient) : IAuthService
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    private const string CodeforcesApiUrl = "https://codeforces.com/api/user.info?handles=";

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();

        if (await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail))
            throw new EmailAlreadyExistsException(request.Email);

        if (!string.IsNullOrWhiteSpace(request.CodeforcesHandle))
            await ValidateCodeforcesHandleAsync(request.CodeforcesHandle);

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(null!, request.Password),
            CodeforcesHandle = request.CodeforcesHandle
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return new AuthResponse(user.Id, user.Email, user.CodeforcesHandle);
    }

    private async Task ValidateCodeforcesHandleAsync(string handle)
    {
        try
        {
            var response = await httpClient.GetAsync($"{CodeforcesApiUrl}{handle}");
            
            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new InvalidCodeforcesHandleException(handle);

            if (!response.IsSuccessStatusCode)
                throw new CodeforcesApiException("Unable to verify Codeforces handle.", (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            throw new CodeforcesApiException("Unable to connect to Codeforces to verify handle.");
        }
    }
}
