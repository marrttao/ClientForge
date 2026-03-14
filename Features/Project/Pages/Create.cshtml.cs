using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ClientForge.Data;
using ClientForge.Features.Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClientForge.Features.Project.Pages;

[Authorize]
public class Create : PageModel
{
    private readonly AppDbContext _db;
    public Create(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();

    public string? SuccessMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Введите название проекта")]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userIdClaim = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid"))?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return RedirectToPage("/Pages/Authentication");
        }

        var role = (User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role"))?.Value ?? "guest";

        // Guest creates projects with PendingApproval status
        var status = role == "guest" ? ProjectStatus.PendingApproval : ProjectStatus.NotStarted;

        var project = new Models.Project
        {
            Id = Guid.NewGuid(),
            Name = Input.Name,
            Description = Input.Description ?? "",
            Status = status,
            DueDate = Input.DueDate.HasValue ? DateTime.SpecifyKind(Input.DueDate.Value, DateTimeKind.Utc) : null,
            ClientId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        if (role == "guest")
        {
            SuccessMessage = "Project sent for admin approval!";
            ModelState.Clear();
            Input = new InputModel();
            return Page();
        }

        return RedirectToPage("/Features/Project/Pages/Index");
    }
}

