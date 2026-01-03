using CodeforcesRandomizer.Models;

namespace CodeforcesRandomizer.Services;

/// <summary>
/// Defines the contract for interacting with the Codeforces API.
/// </summary>
public interface ICodeforcesService
{
    /// <summary>
    /// Gets a specified number of random unsolved problems for a user.
    /// </summary>
    /// <param name="username">The Codeforces username.</param>
    /// <param name="count">The number of problems to return.</param>
    /// <returns>A collection of unsolved problems.</returns>
    Task<IEnumerable<ProblemDto>> GetRandomUnsolvedProblemsAsync(string username, int count = 5);
}
