using ClientForge.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClientForge.Features.Admin.Pages;

[Authorize(Roles = "admin")]
public class Index : PageModel
{
    private readonly AppDbContext _db;
    public Index(AppDbContext db) => _db = db;

    public int TotalUsers { get; set; }
    public int TotalProjects { get; set; }
    public int TotalTasks { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int PendingTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingProjects { get; set; }
    public int PendingApprovalTasks { get; set; }

    public async Task OnGetAsync()
    {
        TotalUsers = await _db.Users.CountAsync();
        TotalProjects = await _db.Projects.CountAsync();
        TotalTasks = await _db.Tasks.CountAsync();
        ActiveProjects = await _db.Projects.CountAsync(p => p.Status == Features.Project.Models.ProjectStatus.InProgress);
        CompletedProjects = await _db.Projects.CountAsync(p => p.Status == Features.Project.Models.ProjectStatus.Completed);
        PendingTasks = await _db.Tasks.CountAsync(t => t.Status == Features.Project.Models.TaskStatus.New || t.Status == Features.Project.Models.TaskStatus.InProgress);
        CompletedTasks = await _db.Tasks.CountAsync(t => t.Status == Features.Project.Models.TaskStatus.Completed);
        PendingProjects = await _db.Projects.CountAsync(p => p.Status == Features.Project.Models.ProjectStatus.PendingApproval);
        PendingApprovalTasks = await _db.Tasks.CountAsync(t => t.Status == Features.Project.Models.TaskStatus.PendingApproval);
    }
}

