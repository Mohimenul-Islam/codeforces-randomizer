using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using CodeforcesRandomizer.Data;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CodeforcesRandomizer.Services;

public class AuthService(AppDbContext dbContext, HttpClient httpClient, IConfiguration config) : IAuthService
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

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null)
            throw new InvalidCredentialsException();

        var result = _passwordHasher.VerifyHashedPassword(null!, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();

        var token = GenerateJwtToken(user);
        return new AuthResponse(user.Id, user.Email, user.CodeforcesHandle, token);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:ExpiryMinutes"] ?? "60"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthResponse?> GetByIdAsync(int userId)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user is null)
            return null;

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
