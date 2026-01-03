namespace CodeforcesRandomizer.Models.Codeforces;

/// <summary>
/// Generic wrapper for Codeforces API responses.
/// </summary>
/// <typeparam name="T">The type of the result payload.</typeparam>
public record CfApiResponse<T>(
    string Status,
    T Result
);

/// <summary>
/// Response structure for the problemset.problems endpoint.
/// </summary>
public record CfProblemsResult(
    CfProblem[] Problems,
    object[] ProblemStatistics
);
