using System.Net;
using System.Security.Claims;
using CodeforcesRandomizer.Data;
using CodeforcesRandomizer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CodeforcesRandomizer.Pages.Auth;

public class RegisterModel(AppDbContext dbContext, HttpClient httpClient) : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;
    
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    
    [BindProperty]
    public string? CodeforcesHandle { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public void OnGet()
    {
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var normalizedEmail = Email.ToLowerInvariant();
        
        if (await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail))
        {
            ErrorMessage = "Email already registered.";
            return Page();
        }
        
        if (!string.IsNullOrWhiteSpace(CodeforcesHandle))
        {
            var valid = await ValidateCodeforcesHandleAsync(CodeforcesHandle);
            if (!valid)
            {
                ErrorMessage = $"Codeforces handle '{CodeforcesHandle}' does not exist.";
                return Page();
            }
        }
        
        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = hasher.HashPassword(null!, Password),
            CodeforcesHandle = CodeforcesHandle
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.CodeforcesHandle ?? user.Email)
        };
        
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        
        return RedirectToPage("/Dashboard");
    }
    
    private async Task<bool> ValidateCodeforcesHandleAsync(string handle)
    {
        try
        {
            var response = await httpClient.GetAsync($"https://codeforces.com/api/user.info?handles={handle}");
            return response.StatusCode != HttpStatusCode.BadRequest && response.IsSuccessStatusCode;
        }
        catch
        {
            return true;
        }
    }
}
