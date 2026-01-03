using System.Text.Json;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Models.Codeforces;

namespace CodeforcesRandomizer.Services;

/// <summary>
/// Service for interacting with the Codeforces API.
/// </summary>
public class CodeforcesService : ICodeforcesService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string BaseUrl = "https://codeforces.com/api";

    public CodeforcesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProblemDto>> GetRandomUnsolvedProblemsAsync(string username, int count = 5)
    {
        // Get all problems from Codeforces
        var allProblems = await GetAllProblemsAsync();

        // Get user's submission history
        var userSubmissions = await GetUserSubmissionsAsync(username);

        // Find problems the user has solved (verdict = "OK")
        var solvedProblemIds = userSubmissions
            .Where(s => s.Verdict == "OK")
            .Select(s => s.Problem.ProblemId)
            .ToHashSet();

        // Get unsolved problems (never solved, may or may not have been attempted)
        var unsolvedProblems = allProblems
            .Where(p => !solvedProblemIds.Contains(p.ProblemId))
            .ToList();

        // Randomly select the requested number of problems
        var randomProblems = unsolvedProblems
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .Select(MapToDto)
            .ToList();

        return randomProblems;
    }

    /// <summary>
    /// Fetches all problems from Codeforces problemset.
    /// </summary>
    private async Task<IEnumerable<CfProblem>> GetAllProblemsAsync()
    {
        var response = await _httpClient.GetStringAsync($"{BaseUrl}/problemset.problems");
        var result = JsonSerializer.Deserialize<CfApiResponse<CfProblemsResult>>(response, _jsonOptions);

        return result?.Result.Problems ?? [];
    }

    /// <summary>
    /// Fetches all submissions for a specific user.
    /// </summary>
    private async Task<IEnumerable<CfSubmission>> GetUserSubmissionsAsync(string username)
    {
        var response = await _httpClient.GetStringAsync($"{BaseUrl}/user.status?handle={username}");
        var result = JsonSerializer.Deserialize<CfApiResponse<CfSubmission[]>>(response, _jsonOptions);

        return result?.Result ?? [];
    }

    /// <summary>
    /// Maps a Codeforces problem to a DTO.
    /// </summary>
    private static ProblemDto MapToDto(CfProblem problem)
    {
        return new ProblemDto(
            problem.ProblemId,
            problem.Name,
            problem.Rating,
            problem.Tags,
            problem.Url
        );
    }
}
