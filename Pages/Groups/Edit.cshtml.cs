using System.Security.Claims;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeforcesRandomizer.Pages.Groups;

[Authorize]
public class EditModel(IGroupsService groupsService) : PageModel
{
    [BindProperty]
    public string Name { get; set; } = string.Empty;
    
    [BindProperty]
    public string Usernames { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
    
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var group = await groupsService.GetByIdAsync(id, userId);
        
        if (group is null)
            return RedirectToPage("/Dashboard");
        
        Name = group.Name;
        Usernames = string.Join("\n", group.Usernames);
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(int id)
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
            await groupsService.UpdateAsync(id, userId, Name.Trim(), usernameList);
            return RedirectToPage("/Dashboard");
        }
        catch (GroupNotFoundException)
        {
            return RedirectToPage("/Dashboard");
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
