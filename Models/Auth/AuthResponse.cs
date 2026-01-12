namespace CodeforcesRandomizer.Models.Auth;

public record AuthResponse(
    int Id,
    string Email,
    string? CodeforcesHandle,
    string? Token = null
);
