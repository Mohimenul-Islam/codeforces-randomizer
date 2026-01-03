namespace CodeforcesRandomizer.Models.Codeforces;

/// <summary>
/// Represents a problem from the Codeforces API.
/// </summary>
public record CfProblem(
    int ContestId,
    string Index,
    string Name,
    int? Rating,
    string[] Tags
)
{
    /// <summary>
    /// Gets the unique problem identifier (e.g., "1500C").
    /// </summary>
    public string ProblemId => $"{ContestId}{Index}";

    /// <summary>
    /// Gets the direct URL to the problem on Codeforces.
    /// </summary>
    public string Url => $"https://codeforces.com/problemset/problem/{ContestId}/{Index}";
}
