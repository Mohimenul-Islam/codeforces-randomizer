using System.Security.Claims;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeforcesRandomizer.Pages;

[Authorize]
public class DashboardModel(IGroupsService groupsService) : PageModel
{
    public List<PracticeGroup> Groups { get; set; } = [];
    
    public async Task OnGetAsync()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Groups = (await groupsService.GetUserGroupsAsync(userId)).ToList();
    }
}
