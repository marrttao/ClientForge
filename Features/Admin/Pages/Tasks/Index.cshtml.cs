using ClientForge.Data;
using ClientForge.Features.Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClientForge.Features.Admin.Pages.Tasks;

[Authorize(Roles = "admin")]
public class Index : PageModel
{
    private readonly AppDbContext _db;
    public Index(AppDbContext db) => _db = db;

    public List<TaskModel> Tasks { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        var query = _db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Worker)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(s)
                                     || t.Project.Name.ToLower().Contains(s)
                                     || t.Worker.Login.ToLower().Contains(s));
        }
        Tasks = await query.OrderByDescending(t => t.DueDate).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task != null)
        {
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}

