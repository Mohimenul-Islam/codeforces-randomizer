using CodeforcesRandomizer.Data;
using CodeforcesRandomizer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CodeforcesRandomizer.Pages.Auth;

public class ResetPasswordModel(AppDbContext dbContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;
    
    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;
    
    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
    
    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(Token))
        {
            return RedirectToPage("/Auth/Login");
        }
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }
        
        var user = await dbContext.Users.FirstOrDefaultAsync(u => 
            u.ResetToken == Token && 
            u.ResetTokenExpiry > DateTime.UtcNow);
            
        if (user == null)
        {
            ErrorMessage = "Invalid or expired reset token.";
            return Page();
        }
        
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(null!, NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        
        await dbContext.SaveChangesAsync();
        
        return RedirectToPage("/Auth/Login");
    }
}
