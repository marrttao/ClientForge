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
public class Edit : PageModel
{
    private readonly AppDbContext _db;
    public Edit(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public int Id { get; set; }
    public SelectList ProjectList { get; set; } = null!;
    public SelectList WorkerList { get; set; } = null!;

    public class InputModel
    {
        [Required, MaxLength(200)] public string Name { get; set; } = "";
        public string? Description { get; set; }
        [Required] public DateTime DueDate { get; set; }
        [Required] public Guid ProjectId { get; set; }
        [Required] public Guid WorkerId { get; set; }
        public TaskStatus Status { get; set; }
        public string? SubmissionResult { get; set; }
        public string? ReviewComment { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return RedirectToPage("/Features/Admin/Pages/Tasks/Index");

        Id = id;
        Input = new InputModel
        {
            Name = task.Name,
            Description = task.Description,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId,
            WorkerId = task.WorkerId,
            Status = task.Status,
            SubmissionResult = task.SubmissionResult,
            ReviewComment = task.ReviewComment
        };
        await LoadSelectsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSelectsAsync();
        if (!ModelState.IsValid) return Page();

        var task = await _db.Tasks.FindAsync(Id);
        if (task == null) return RedirectToPage("/Features/Admin/Pages/Tasks/Index");

        task.Name = Input.Name;
        task.Description = Input.Description ?? "";
        task.DueDate = DateTime.SpecifyKind(Input.DueDate, DateTimeKind.Utc);
        task.ProjectId = Input.ProjectId;
        task.WorkerId = Input.WorkerId;
        task.Status = Input.Status;
        task.SubmissionResult = Input.SubmissionResult;
        task.ReviewComment = Input.ReviewComment;

        await _db.SaveChangesAsync();
        return RedirectToPage("/Features/Admin/Pages/Tasks/Index");
    }

    private async Task LoadSelectsAsync()
    {
        var projects = await _db.Projects.OrderBy(p => p.Name).ToListAsync();
        ProjectList = new SelectList(projects.Select(p => new { p.Id, Display = p.Name }), "Id", "Display");

        var workers = await _db.Users.OrderBy(u => u.Login).ToListAsync();
        WorkerList = new SelectList(workers.Select(u => new { u.Id, Display = $"{u.Name} {u.Surname} ({u.Login}) [{u.Role}]" }), "Id", "Display");
    }
}

