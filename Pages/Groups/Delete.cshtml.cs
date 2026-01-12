using System.Security.Claims;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeforcesRandomizer.Pages.Groups;

[Authorize]
public class DeleteModel(IGroupsService groupsService) : PageModel
{
    public string GroupName { get; set; } = string.Empty;
    
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var group = await groupsService.GetByIdAsync(id, userId);
        
        if (group is null)
            return RedirectToPage("/Dashboard");
        
        GroupName = group.Name;
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        try
        {
            await groupsService.DeleteAsync(id, userId);
        }
        catch (GroupNotFoundException)
        {
        }
        
        return RedirectToPage("/Dashboard");
    }
}
