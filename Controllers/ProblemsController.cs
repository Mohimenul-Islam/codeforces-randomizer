using System.ComponentModel.DataAnnotations;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeforcesRandomizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProblemsController(ICodeforcesService codeforcesService) : ControllerBase
{
    [HttpGet("random/{username}")]
    public async Task<ActionResult<IEnumerable<ProblemDto>>> GetRandomUnsolved(
        [FromRoute, RegularExpression(@"^[a-zA-Z0-9_]{3,24}$")] string username,
        [FromQuery, Range(1, 365)] int count = 5,
        [FromQuery, Range(800, 3500)] int minRating = 800,
        [FromQuery, Range(800, 3500)] int maxRating = 2000)
    {
        try
        {
            var problems = await codeforcesService.GetRandomUnsolvedProblemsAsync(username, count, minRating, maxRating);
            return Ok(problems);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "User Not Found",
                Status = 404,
                Detail = ex.Message
            });
        }
        catch (CodeforcesApiException ex)
        {
            return StatusCode(502, new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.3",
                Title = "Bad Gateway",
                Status = 502,
                Detail = ex.Message
            });
        }
    }
}
