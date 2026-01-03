namespace CodeforcesRandomizer.Models;

/// <summary>
/// Data transfer object representing a Codeforces problem for API responses.
/// </summary>
public record ProblemDto(
    string ProblemId,
    string Name,
    int? Rating,
    string[] Tags,
    string Url
);
