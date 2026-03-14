using System.Security.Claims;
using ClientForge.Data;
using ClientForge.Features.Project.Models;
using ClientForge.Features.User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskStatus = ClientForge.Features.Project.Models.TaskStatus;

namespace ClientForge.Features.Project.Pages;

public class Task : PageModel
{
    private readonly AppDbContext _dbContext;

    public Task(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Models.Project Project { get; set; } = null!;
    public List<TaskModel> Tasks { get; set; } = new();
    public string UserRole { get; set; } = "guest";
    public Guid CurrentUserId { get; set; }

    [BindProperty] public int TaskId { get; set; }
    [BindProperty] public string? SubmissionResult { get; set; }
    [BindProperty] public string? ReviewComment { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid projectId)
    {
        var userIdClaim = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid"))?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return RedirectToPage("/Pages/Authentication");

        CurrentUserId = userId;
        UserRole = (User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role"))?.Value ?? "guest";

        Project = await _dbContext.Projects
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (Project == null)
            return RedirectToPage("/Features/Project/Pages/Index");

        var tasksQuery = _dbContext.Tasks
            .Include(t => t.Worker)
            .Where(t => t.ProjectId == projectId);

        // user видит только свои задачи
        if (UserRole == "user")
            tasksQuery = tasksQuery.Where(t => t.WorkerId == userId);

        Tasks = await tasksQuery
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        return Page();
    }

    // user отправляет результат задачи на рассмотрение
    public async Task<IActionResult> OnPostSubmitAsync(Guid projectId)
    {
        var userIdClaim = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid"))?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return RedirectToPage("/Pages/Authentication");

        var role = (User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role"))?.Value ?? "guest";
        if (role != "user")
            return Forbid();

        var task = await _dbContext.Tasks.FindAsync(TaskId);
        if (task == null || task.ProjectId != projectId)
            return NotFound();

        // Нельзя действовать с чужой задачей
        if (task.WorkerId != userId)
            return Forbid();

        if (task.Status != TaskStatus.New && task.Status != TaskStatus.InProgress && task.Status != TaskStatus.NeedsRework)
            return BadRequest();

        task.SubmissionResult = SubmissionResult;
        task.Status = TaskStatus.UnderReview;
        await _dbContext.SaveChangesAsync();

        return RedirectToPage(new { projectId });
    }

    // User starts working on a task
    public async Task<IActionResult> OnPostStartWorkAsync(Guid projectId)
    {
        var userIdClaim = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid"))?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return RedirectToPage("/Pages/Authentication");

        var role = (User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role"))?.Value ?? "guest";
        if (role != "user")
            return Forbid();

        var task = await _dbContext.Tasks.FindAsync(TaskId);
        if (task == null || task.ProjectId != projectId)
            return NotFound();

        // Нельзя действовать с чужой задачей
        if (task.WorkerId != userId)
            return Forbid();

        if (task.Status != TaskStatus.New)
            return BadRequest();

        task.Status = TaskStatus.InProgress;
        await _dbContext.SaveChangesAsync();

        return RedirectToPage(new { projectId });
    }

    // guest рассматривает задачу: одобрить
    public async Task<IActionResult> OnPostApproveAsync(Guid projectId)
    {
        var userIdClaim = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid"))?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return RedirectToPage("/Pages/Authentication");

        var role = (User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role"))?.Value ?? "guest";
        if (role != "guest")
            return Forbid();

        var task = await _dbContext.Tasks.FindAsync(TaskId);
        if (task == null || task.ProjectId != projectId)
            return NotFound();

        if (task.Status != TaskStatus.UnderReview)
            return BadRequest();

        task.ReviewComment = ReviewComment;
        task.Status = TaskStatus.Completed;
        await _dbContext.SaveChangesAsync();

        return RedirectToPage(new { projectId });
    }

    // guest рассматривает задачу: отклонить (нужна доработка)
    public async Task<IActionResult> OnPostRejectAsync(Guid projectId)
    {
        var userIdClaim = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid"))?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return RedirectToPage("/Pages/Authentication");

        var role = (User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role"))?.Value ?? "guest";
        if (role != "guest")
            return Forbid();

        var task = await _dbContext.Tasks.FindAsync(TaskId);
        if (task == null || task.ProjectId != projectId)
            return NotFound();

        if (task.Status != TaskStatus.UnderReview)
            return BadRequest();

        task.ReviewComment = ReviewComment;
        task.Status = TaskStatus.NeedsRework;
        await _dbContext.SaveChangesAsync();

        return RedirectToPage(new { projectId });
    }
}