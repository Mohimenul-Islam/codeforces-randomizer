using System.ComponentModel.DataAnnotations;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeforcesRandomizer.Controllers;

/// <summary>
/// Controller for managing Codeforces problem recommendations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProblemsController : ControllerBase
{
    private readonly ICodeforcesService _codeforcesService;

    public ProblemsController(ICodeforcesService codeforcesService)
    {
        _codeforcesService = codeforcesService;
    }

    /// <summary>
    /// Gets random unsolved problems for a Codeforces user.
    /// </summary>
    /// <param name="username">The Codeforces username (3-24 characters, alphanumeric with underscores).</param>
    /// <param name="count">Number of problems to return (default: 5).</param>
    /// <returns>A list of random unsolved problems.</returns>
    [HttpGet("random/{username}")]
    public async Task<ActionResult<IEnumerable<ProblemDto>>> GetRandomUnsolved(
        [FromRoute, Required, RegularExpression(@"^[a-zA-Z0-9_]{3,24}$", 
            ErrorMessage = "Username must be 3-24 characters and contain only letters, numbers, and underscores.")]
        string username,
        [FromQuery] int count = 5)
    {
        var problems = await _codeforcesService.GetRandomUnsolvedProblemsAsync(username, count);
        return Ok(problems);
    }
}
