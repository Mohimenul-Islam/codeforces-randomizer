using System.Security.Claims;
using CodeforcesRandomizer.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CodeforcesRandomizer.Models;

namespace CodeforcesRandomizer.Pages.Auth;

public class LoginModel(AppDbContext dbContext) : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;
    
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
    
    public void OnGet()
    {
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var normalizedEmail = Email.ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        
        if (user is null)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }
        
        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(null!, user.PasswordHash, Password);
        
        if (result == PasswordVerificationResult.Failed)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }
        
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
}
