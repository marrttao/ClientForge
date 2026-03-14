using System.ComponentModel.DataAnnotations;
using ClientForge.Data;
using ClientForge.Features.Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClientForge.Features.Admin.Pages.Projects;

[Authorize(Roles = "admin")]
public class Edit : PageModel
{
    private readonly AppDbContext _db;
    public Edit(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public Guid Id { get; set; }
    public SelectList ClientList { get; set; } = null!;

    public class InputModel
    {
        [Required, MaxLength(200)] public string Name { get; set; } = "";
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? FinishedAt { get; set; }
        [Required] public Guid ClientId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return RedirectToPage("/Features/Admin/Pages/Projects/Index");

        Id = id;
        Input = new InputModel
        {
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            DueDate = project.DueDate,
            FinishedAt = project.FinishedAt,
            ClientId = project.ClientId
        };
        await LoadClientsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadClientsAsync();
        if (!ModelState.IsValid) return Page();

        var project = await _db.Projects.FindAsync(Id);
        if (project == null) return RedirectToPage("/Features/Admin/Pages/Projects/Index");

        project.Name = Input.Name;
        project.Description = Input.Description ?? "";
        project.Status = Input.Status;
        project.DueDate = Input.DueDate.HasValue ? DateTime.SpecifyKind(Input.DueDate.Value, DateTimeKind.Utc) : null;
        project.FinishedAt = Input.FinishedAt.HasValue ? DateTime.SpecifyKind(Input.FinishedAt.Value, DateTimeKind.Utc) : null;
        project.ClientId = Input.ClientId;

        await _db.SaveChangesAsync();
        return RedirectToPage("/Features/Admin/Pages/Projects/Index");
    }

    private async Task LoadClientsAsync()
    {
        var users = await _db.Users.OrderBy(u => u.Login).ToListAsync();
        ClientList = new SelectList(users.Select(u => new { u.Id, Display = $"{u.Name} {u.Surname} ({u.Login}) [{u.Role}]" }), "Id", "Display");
    }
}

