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
public class Create : PageModel
{
    private readonly AppDbContext _db;
    public Create(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();
    public SelectList ProjectList { get; set; } = null!;
    public SelectList WorkerList { get; set; } = null!;

    public class InputModel
    {
        [Required, MaxLength(200)] public string Name { get; set; } = "";
        public string? Description { get; set; }
        [Required] public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
        [Required] public Guid ProjectId { get; set; }
        [Required] public Guid WorkerId { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.New;
    }

    public async Task OnGetAsync()
    {
        await LoadSelectsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSelectsAsync();
        if (!ModelState.IsValid) return Page();

        var task = new TaskModel
        {
            Name = Input.Name,
            Description = Input.Description ?? "",
            DueDate = DateTime.SpecifyKind(Input.DueDate, DateTimeKind.Utc),
            ProjectId = Input.ProjectId,
            WorkerId = Input.WorkerId,
            Status = Input.Status
        };

        _db.Tasks.Add(task);
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

