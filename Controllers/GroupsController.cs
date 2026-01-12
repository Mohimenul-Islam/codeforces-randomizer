using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeforcesRandomizer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController(IGroupsService groupsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupResponse>>> GetAll()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var groups = await groupsService.GetUserGroupsAsync(userId.Value);
        return Ok(groups.Select(g => new GroupResponse(g.Id, g.Name, g.Usernames, g.CreatedAt)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GroupResponse>> GetById(int id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var group = await groupsService.GetByIdAsync(id, userId.Value);
        if (group is null)
            return NotFound();

        return Ok(new GroupResponse(group.Id, group.Name, group.Usernames, group.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<GroupResponse>> Create([FromBody] GroupRequest request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "Group name is required."
            });

        if (request.Usernames.Count == 0)
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "At least one username is required."
            });

        try
        {
            var group = await groupsService.CreateAsync(userId.Value, request.Name, request.Usernames);
            var response = new GroupResponse(group.Id, group.Name, group.Usernames, group.CreatedAt);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, response);
        }
        catch (GroupLimitExceededException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = ex.Message
            });
        }
        catch (GroupNameExistsException ex)
        {
            return Conflict(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Title = "Conflict",
                Status = 409,
                Detail = ex.Message
            });
        }
        catch (UserNotFoundException ex)
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

    [HttpPut("{id}")]
    public async Task<ActionResult<GroupResponse>> Update(int id, [FromBody] GroupRequest request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "Group name is required."
            });

        if (request.Usernames.Count == 0)
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "At least one username is required."
            });

        try
        {
            var group = await groupsService.UpdateAsync(id, userId.Value, request.Name, request.Usernames);
            return Ok(new GroupResponse(group.Id, group.Name, group.Usernames, group.CreatedAt));
        }
        catch (GroupNotFoundException)
        {
            return NotFound();
        }
        catch (GroupNameExistsException ex)
        {
            return Conflict(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Title = "Conflict",
                Status = 409,
                Detail = ex.Message
            });
        }
        catch (UserNotFoundException ex)
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            await groupsService.DeleteAsync(id, userId.Value);
            return NoContent();
        }
        catch (GroupNotFoundException)
        {
            return NotFound();
        }
    }

    private int? GetUserId()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub) 
            ?? User.FindFirst(ClaimTypes.NameIdentifier);
        
        if (claim is null || !int.TryParse(claim.Value, out var userId))
            return null;
        
        return userId;
    }
}

public record GroupRequest(string Name, List<string> Usernames);
public record GroupResponse(int Id, string Name, List<string> Usernames, DateTime CreatedAt);
