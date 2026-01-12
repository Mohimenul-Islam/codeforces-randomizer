using System.ComponentModel.DataAnnotations;

namespace CodeforcesRandomizer.Models.Auth;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)]
    string Email,

    [Required, MinLength(8), MaxLength(100)]
    string Password,

    [MaxLength(24)]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$")]
    string? CodeforcesHandle
);
