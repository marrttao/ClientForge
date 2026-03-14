using ClientForge.Data;
using ClientForge.Features.Project.Models;
using ClientForge.Features.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClientForge.Features.Admin.Pages.Projects;

[Authorize(Roles = "admin")]
public class Index : PageModel
{
    private readonly AppDbContext _db;
    public Index(AppDbContext db) => _db = db;

    public List<Project.Models.Project> Projects { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        var query = _db.Projects.Include(p => p.Client).Include(p => p.Tasks).AsQueryable();
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s)
                                     || p.Client.Login.ToLower().Contains(s)
                                     || p.Client.Name.ToLower().Contains(s));
        }
        Projects = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project != null)
        {
            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}

