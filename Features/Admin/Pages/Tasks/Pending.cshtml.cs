using System.ComponentModel.DataAnnotations;
using ClientForge.Data;
using ClientForge.Features.Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskStatus = ClientForge.Features.Project.Models.TaskStatus;

namespace ClientForge.Features.Admin.Pages.Tasks;

[Authorize(Roles = "admin")]
public class Pending : PageModel
{
    private readonly AppDbContext _db;
    public Pending(AppDbContext db) => _db = db;

    public List<TaskModel> PendingTasks { get; set; } = new();
    public SelectList WorkerList { get; set; } = null!;

    [BindProperty] public int TaskId { get; set; }
    [BindProperty] public Guid WorkerId { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync()
    {
        if (WorkerId == Guid.Empty)
        {
            await LoadDataAsync();
            ModelState.AddModelError("WorkerId", "Необходимо выбрать исполнителя.");
            return Page();
        }

        var task = await _db.Tasks.FindAsync(TaskId);
        if (task != null && task.Status == TaskStatus.PendingApproval)
        {
            task.WorkerId = WorkerId;
            task.Status = TaskStatus.New;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task != null && task.Status == TaskStatus.PendingApproval)
        {
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task LoadDataAsync()
    {
        PendingTasks = await _db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Worker)
            .Where(t => t.Status == TaskStatus.PendingApproval)
            .OrderByDescending(t => t.DueDate)
            .ToListAsync();

        var workers = await _db.Users
            .Where(u => u.Role == Features.User.Models.Role.user || u.Role == Features.User.Models.Role.admin)
            .OrderBy(u => u.Login)
            .ToListAsync();
        WorkerList = new SelectList(
            workers.Select(u => new { u.Id, Display = $"{u.Name} {u.Surname} ({u.Login})" }),
            "Id", "Display");
    }
}

