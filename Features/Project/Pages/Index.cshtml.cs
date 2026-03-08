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

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return RedirectToPage("/Pages/Authentication");
        }

        Projects = await _dbContext.Projects
            .Include(p => p.Client)
            .Include(p => p.Tasks)
            .Where(p => p.ClientId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Page();
    }
}