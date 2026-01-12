using CodeforcesRandomizer.Data;
using CodeforcesRandomizer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CodeforcesRandomizer.Pages.Auth;

public class ForgotPasswordModel(AppDbContext dbContext, IEmailService emailService) : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;
    
    public bool Success { get; set; }
    
    public void OnGet()
    {
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == Email.ToLowerInvariant());
        
        if (user != null)
        {
            var token = Guid.NewGuid().ToString("N");
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await dbContext.SaveChangesAsync();
            
            var resetLink = Url.Page(
                "/Auth/ResetPassword",
                pageHandler: null,
                values: new { token },
                protocol: Request.Scheme);
                
            await emailService.SendPasswordResetEmailAsync(user.Email, resetLink!);
        }
        
        // Always show success to prevent email enumeration
        Success = true;
        return Page();
    }
}
