using System.Security.Claims;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeforcesRandomizer.Pages.Groups;

[Authorize]
public class CreateModel(IGroupsService groupsService) : PageModel
{
    [BindProperty]
    public string Name { get; set; } = string.Empty;
    
    [BindProperty]
    public string Usernames { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
    
    public void OnGet()
    {
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var usernameList = Usernames
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToList();
        
        if (usernameList.Count == 0)
        {
            ErrorMessage = "At least one username is required.";
            return Page();
        }
        
        try
        {
            await groupsService.CreateAsync(userId, Name.Trim(), usernameList);
            return RedirectToPage("/Dashboard");
        }
        catch (GroupLimitExceededException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (GroupNameExistsException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (UserNotFoundException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (CodeforcesApiException)
        {
            ErrorMessage = "Could not verify Codeforces handles. Please try again.";
            return Page();
        }
    }
}
