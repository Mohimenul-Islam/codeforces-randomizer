using System.Security.Claims;
using CodeforcesRandomizer.Exceptions;
using CodeforcesRandomizer.Models;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeforcesRandomizer.Pages;

[Authorize]
public class SearchModel(IGroupsService groupsService, ICodeforcesService codeforcesService) : PageModel
{
    public List<PracticeGroup> Groups { get; set; } = [];
    public List<ProblemDto> Problems { get; set; } = [];
    public bool Searched { get; set; }
    public string? ErrorMessage { get; set; }
    
    [BindProperty]
    public int? SelectedGroupId { get; set; }
    
    [BindProperty]
    public string? ManualUsernames { get; set; }
    
    [BindProperty]
    public int MinRating { get; set; } = 800;
    
    [BindProperty]
    public int MaxRating { get; set; } = 2000;
    
    [BindProperty]
    public int Count { get; set; } = 5;
    
    public async Task OnGetAsync(int? groupId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Groups = (await groupsService.GetUserGroupsAsync(userId)).ToList();
        
        if (groupId.HasValue)
            SelectedGroupId = groupId;
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Groups = (await groupsService.GetUserGroupsAsync(userId)).ToList();
        Searched = true;
        
        var usernames = new List<string>();
        
        if (SelectedGroupId.HasValue)
        {
            var group = await groupsService.GetByIdAsync(SelectedGroupId.Value, userId);
            if (group != null)
                usernames = group.Usernames;
        }
        else if (!string.IsNullOrWhiteSpace(ManualUsernames))
        {
            usernames = ManualUsernames
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();
        }
        
        if (usernames.Count == 0)
        {
            ErrorMessage = "Select a group or enter at least one username.";
            return Page();
        }
        
        try
        {
            Problems = (await codeforcesService.GetRandomUnsolvedProblemsAsync(
                usernames, Count, MinRating, MaxRating)).ToList();
        }
        catch (UserNotFoundException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (CodeforcesApiException)
        {
            ErrorMessage = "Could not connect to Codeforces. Please try again.";
        }
        
        return Page();
    }
}
