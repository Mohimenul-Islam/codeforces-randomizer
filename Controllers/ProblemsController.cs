using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeforcesRandomizer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProblemsController(ICodeforcesService codeforcesService) : ControllerBase
{
    [HttpPost("random")]
    public async Task<ActionResult<IEnumerable<ProblemDto>>> GetRandomUnsolved(
        [FromBody] ProblemRequest request)
    {
        if (request.Usernames.Count == 0)
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "At least 1 username is required."
            });

        if (request.Usernames.Count > 20)
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "Maximum 20 users allowed."
            });

        try
        {
            var problems = await codeforcesService.GetRandomUnsolvedProblemsAsync(
                request.Usernames, request.Count, request.MinRating, request.MaxRating);
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

public record ProblemRequest(
    List<string> Usernames,
    int Count = 5,
    int MinRating = 800,
    int MaxRating = 2000
);
