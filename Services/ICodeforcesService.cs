using CodeforcesRandomizer.Models;

namespace CodeforcesRandomizer.Services;

public interface ICodeforcesService
{
    Task<IEnumerable<ProblemDto>> GetRandomUnsolvedProblemsAsync(
        string username, int count = 5, int minRating = 800, int maxRating = 2000);
}
