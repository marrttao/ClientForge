using System.ComponentModel.DataAnnotations;
using ClientForge.Data;
using ClientForge.Features.Project.Models;
using ClientForge.Features.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClientForge.Features.Admin.Pages.Projects;

[Authorize(Roles = "admin")]
public class Create : PageModel
{
    private readonly AppDbContext _db;
    public Create(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();
    public SelectList ClientList { get; set; } = null!;

    public class InputModel
    {
        [Required, MaxLength(200)] public string Name { get; set; } = "";
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.NotStarted;
        public DateTime? DueDate { get; set; }
        [Required] public Guid ClientId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadClientsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadClientsAsync();
        if (!ModelState.IsValid) return Page();

        var project = new Project.Models.Project
        {
            Id = Guid.NewGuid(),
            Name = Input.Name,
            Description = Input.Description ?? "",
            Status = Input.Status,
            DueDate = Input.DueDate.HasValue ? DateTime.SpecifyKind(Input.DueDate.Value, DateTimeKind.Utc) : null,
            ClientId = Input.ClientId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return RedirectToPage("/Features/Admin/Pages/Projects/Index");
    }

    private async Task LoadClientsAsync()
    {
        var users = await _db.Users.OrderBy(u => u.Login).ToListAsync();
        ClientList = new SelectList(users.Select(u => new { u.Id, Display = $"{u.Name} {u.Surname} ({u.Login}) [{u.Role}]" }), "Id", "Display");
    }
}

