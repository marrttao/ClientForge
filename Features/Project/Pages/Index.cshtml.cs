using System.Security.Claims;
using ClientForge.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClientForge.Features.Project.Pages;

public class Index : PageModel
{
    private readonly AppDbContext _dbContext;

    public Index(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Models.Project> Projects { get; set; } = new();

    public string UserRole { get; set; } = "guest";

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdClaim = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid"))?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return RedirectToPage("/Pages/Authentication");
        }

        UserRole = (User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role"))?.Value ?? "guest";

        if (UserRole == "admin")
        {
            // Администратор видит все проекты
            Projects = await _dbContext.Projects
                .Include(p => p.Client)
                .Include(p => p.Tasks)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        else if (UserRole == "user")
        {
            // Пользователь с ролью user видит проекты, в которых у него есть задача
            Projects = await _dbContext.Projects
                .Include(p => p.Client)
                .Include(p => p.Tasks)
                .Where(p => p.Tasks.Any(t => t.WorkerId == userId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        else
        {
            // Guest — показываем проекты, где пользователь является клиентом (включая PendingApproval)
            Projects = await _dbContext.Projects
                .Include(p => p.Client)
                .Include(p => p.Tasks)
                .Where(p => p.ClientId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        return Page();
    }
}