using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ClientForge.Data;
using ClientForge.Features.Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskStatus = ClientForge.Features.Project.Models.TaskStatus;

namespace ClientForge.Features.Project.Pages;

[Authorize]
public class CreateTask : PageModel
{
    private readonly AppDbContext _db;
    public CreateTask(AppDbContext db) => _db = db;

    public Models.Project ProjectInfo { get; set; } = null!;

    [BindProperty] public Guid ProjectId { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();

    public string? SuccessMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Введите название задачи")]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Укажите желаемый дедлайн")]
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    }

    public async Task<IActionResult> OnGetAsync(Guid projectId)
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToPage("/Pages/Authentication");

        var project = await _db.Projects.Include(p => p.Client).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return RedirectToPage("/Features/Project/Pages/Index");

        // Only the project client (guest) can propose tasks
        if (project.ClientId != userId.Value) return Forbid();

        ProjectInfo = project;
        ProjectId = projectId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToPage("/Pages/Authentication");

        var project = await _db.Projects.Include(p => p.Client).FirstOrDefaultAsync(p => p.Id == ProjectId);
        if (project == null) return RedirectToPage("/Features/Project/Pages/Index");
        if (project.ClientId != userId.Value) return Forbid();

        ProjectInfo = project;

        if (!ModelState.IsValid) return Page();

        // Guest creates a placeholder worker — admin will reassign.
        // We use the guest's own ID as a temporary WorkerId placeholder since WorkerId is required.
        var task = new TaskModel
        {
            Name = Input.Name,
            Description = Input.Description ?? "",
            DueDate = DateTime.SpecifyKind(Input.DueDate, DateTimeKind.Utc),
            ProjectId = ProjectId,
            WorkerId = userId.Value, // placeholder, admin will reassign
            Status = TaskStatus.PendingApproval
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        SuccessMessage = "Task sent for admin review!";
        ModelState.Clear();
        Input = new InputModel();
        return Page();
    }

    private Guid? GetUserId()
    {
        var claim = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid"))?.Value;
        if (claim != null && Guid.TryParse(claim, out var id)) return id;
        return null;
    }
}

