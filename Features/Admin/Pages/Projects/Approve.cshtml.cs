using System.ComponentModel.DataAnnotations;
using ClientForge.Data;
using ClientForge.Features.Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskStatus = ClientForge.Features.Project.Models.TaskStatus;

namespace ClientForge.Features.Admin.Pages.Projects;

[Authorize(Roles = "admin")]
public class Approve : PageModel
{
    private readonly AppDbContext _db;
    public Approve(AppDbContext db) => _db = db;

    public Project.Models.Project ProjectInfo { get; set; } = null!;

    [BindProperty] public Guid ProjectId { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public SelectList WorkerList { get; set; } = null!;

    public class InputModel
    {
        [Required(ErrorMessage = "Введите название задачи")]
        [MaxLength(200)]
        public string TaskName { get; set; } = "";

        public string? TaskDescription { get; set; }

        [Required(ErrorMessage = "Укажите дедлайн")]
        public DateTime TaskDueDate { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required(ErrorMessage = "Выберите исполнителя")]
        public Guid WorkerId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var project = await _db.Projects.Include(p => p.Client).FirstOrDefaultAsync(p => p.Id == id);
        if (project == null || project.Status != ProjectStatus.PendingApproval)
            return RedirectToPage("/Features/Admin/Pages/Projects/Pending");

        ProjectInfo = project;
        ProjectId = id;
        await LoadWorkersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var project = await _db.Projects.Include(p => p.Client).FirstOrDefaultAsync(p => p.Id == ProjectId);
        if (project == null || project.Status != ProjectStatus.PendingApproval)
            return RedirectToPage("/Features/Admin/Pages/Projects/Pending");

        ProjectInfo = project;
        await LoadWorkersAsync();

        if (!ModelState.IsValid)
            return Page();

        // Create the first task
        var task = new TaskModel
        {
            Name = Input.TaskName,
            Description = Input.TaskDescription ?? "",
            DueDate = DateTime.SpecifyKind(Input.TaskDueDate, DateTimeKind.Utc),
            ProjectId = ProjectId,
            WorkerId = Input.WorkerId,
            Status = TaskStatus.New
        };
        _db.Tasks.Add(task);

        // Approve the project
        project.Status = ProjectStatus.NotStarted;
        await _db.SaveChangesAsync();

        return RedirectToPage("/Features/Admin/Pages/Projects/Pending");
    }

    private async Task LoadWorkersAsync()
    {
        var workers = await _db.Users
            .Where(u => u.Role == Features.User.Models.Role.user || u.Role == Features.User.Models.Role.admin)
            .OrderBy(u => u.Login)
            .ToListAsync();
        WorkerList = new SelectList(
            workers.Select(u => new { u.Id, Display = $"{u.Name} {u.Surname} ({u.Login}) [{u.Role}]" }),
            "Id", "Display");
    }
}

