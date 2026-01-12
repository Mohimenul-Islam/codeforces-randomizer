using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Models.Auth;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeforcesRandomizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
        }
        catch (EmailAlreadyExistsException ex)
        {
            return Conflict(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Title = "Conflict",
                Status = 409,
                Detail = ex.Message
            });
        }
        catch (InvalidCodeforcesHandleException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
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

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await authService.LoginAsync(request);
            return Ok(response);
        }
        catch (InvalidCredentialsException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = 401,
                Detail = ex.Message
            });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponse>> GetMe()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) 
            ?? User.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var user = await authService.GetByIdAsync(userId);
        if (user is null)
            return NotFound();

        return Ok(user);
    }
}
