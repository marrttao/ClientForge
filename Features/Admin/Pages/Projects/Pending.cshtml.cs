using ClientForge.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClientForge.Features.Project.Models;

namespace ClientForge.Features.Admin.Pages.Projects;

[Authorize(Roles = "admin")]
public class Pending : PageModel
{
    private readonly AppDbContext _db;
    public Pending(AppDbContext db) => _db = db;

    public List<Project.Models.Project> PendingProjects { get; set; } = new();

    public async Task OnGetAsync()
    {
        PendingProjects = await _db.Projects
            .Include(p => p.Client)
            .Where(p => p.Status == ProjectStatus.PendingApproval)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null || project.Status != ProjectStatus.PendingApproval)
            return RedirectToPage();

        // Redirect to approve page where admin creates the first task
        return RedirectToPage("/Features/Admin/Pages/Projects/Approve", new { id });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project != null && project.Status == ProjectStatus.PendingApproval)
        {
            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}

