namespace CodeforcesRandomizer.Models.Codeforces;

public record CfApiResponse<T>(string Status, T Result);

public record CfProblemsResult(CfProblem[] Problems, object[] ProblemStatistics);
