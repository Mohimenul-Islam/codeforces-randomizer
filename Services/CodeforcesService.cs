using System.Net;
using System.Text.Json;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Models.Codeforces;

namespace CodeforcesRandomizer.Services;

public class CodeforcesService : ICodeforcesService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string BaseUrl = "https://codeforces.com/api";

    public CodeforcesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<IEnumerable<ProblemDto>> GetRandomUnsolvedProblemsAsync(
        IEnumerable<string> usernames, int count = 5, int minRating = 800, int maxRating = 2000)
    {
        var usernameList = usernames.Distinct().ToList();

        // Fetch all problems and user submissions in parallel
        var allProblemsTask = GetAllProblemsAsync();
        var submissionTasks = usernameList
            .Select(async username => (Username: username, Result: await GetUserSubmissionsSafeAsync(username)))
            .ToList();

        var results = await Task.WhenAll(submissionTasks);
        var allProblems = await allProblemsTask;

        // Check for invalid usernames
        var invalidUsernames = results
            .Where(r => r.Result.IsError)
            .Select(r => r.Username)
            .ToList();

        if (invalidUsernames.Count > 0)
            throw new UserNotFoundException(invalidUsernames);

        // Combine solved problems from ALL users
        var solvedByAnyUser = new HashSet<string>();
        foreach (var (_, result) in results)
        {
            var solvedIds = result.Submissions!
                .Where(s => s.Verdict == "OK")
                .Select(s => s.Problem.ProblemId);
            solvedByAnyUser.UnionWith(solvedIds);
        }

        // Find problems unsolved by ALL users
        var unsolvedByAll = allProblems
            .Where(p => !solvedByAnyUser.Contains(p.ProblemId))
            .Where(p => p.Rating >= minRating && p.Rating <= maxRating)
            .ToList();

        return unsolvedByAll
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .Select(p => new ProblemDto(p.ProblemId, p.Name, p.Rating, p.Tags, p.Url))
            .ToList();
    }

    private async Task<IEnumerable<CfProblem>> GetAllProblemsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/problemset.problems");
            if (!response.IsSuccessStatusCode)
                throw new CodeforcesApiException("Codeforces API unavailable.", (int)response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CfApiResponse<CfProblemsResult>>(content, _jsonOptions);
            return result?.Result.Problems ?? [];
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new CodeforcesApiException("Request timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new CodeforcesApiException("Failed to connect to Codeforces.", ex);
        }
    }

    private async Task<(bool IsError, IEnumerable<CfSubmission>? Submissions)> GetUserSubmissionsSafeAsync(string username)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/user.status?handle={username}");

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return (true, null); // User not found

            if (!response.IsSuccessStatusCode)
                throw new CodeforcesApiException("Codeforces API unavailable.", (int)response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CfApiResponse<CfSubmission[]>>(content, _jsonOptions);
            return (false, result?.Result ?? []);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new CodeforcesApiException("Request timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new CodeforcesApiException("Failed to connect to Codeforces.", ex);
        }
    }
}
