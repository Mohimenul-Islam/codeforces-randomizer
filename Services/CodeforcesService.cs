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
        string username, int count = 5, int minRating = 800, int maxRating = 2000)
    {
        var allProblems = await GetAllProblemsAsync();
        var userSubmissions = await GetUserSubmissionsAsync(username);

        var solvedProblemIds = userSubmissions
            .Where(s => s.Verdict == "OK")
            .Select(s => s.Problem.ProblemId)
            .ToHashSet();

        var unsolvedProblems = allProblems
            .Where(p => !solvedProblemIds.Contains(p.ProblemId))
            .Where(p => p.Rating >= minRating && p.Rating <= maxRating)
            .ToList();

        return unsolvedProblems
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

    private async Task<IEnumerable<CfSubmission>> GetUserSubmissionsAsync(string username)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/user.status?handle={username}");

            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new UserNotFoundException(username);

            if (!response.IsSuccessStatusCode)
                throw new CodeforcesApiException("Codeforces API unavailable.", (int)response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CfApiResponse<CfSubmission[]>>(content, _jsonOptions);
            return result?.Result ?? [];
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
