namespace CodeforcesRandomizer.Models.Codeforces;

/// <summary>
/// Represents a submission from the Codeforces API.
/// </summary>
public record CfSubmission(
    int Id,
    int ContestId,
    CfSubmissionProblem Problem,
    string Verdict
);

/// <summary>
/// Represents the problem information within a submission.
/// </summary>
public record CfSubmissionProblem(
    int ContestId,
    string Index,
    string Name
)
{
    /// <summary>
    /// Gets the unique problem identifier (e.g., "1500C").
    /// </summary>
    public string ProblemId => $"{ContestId}{Index}";
}
