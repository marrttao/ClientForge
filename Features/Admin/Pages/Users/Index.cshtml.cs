using ClientForge.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UserModel = ClientForge.Features.User.Models.User;

namespace ClientForge.Features.Admin.Pages.Users;

[Authorize(Roles = "admin")]
public class Index : PageModel
{
    private readonly AppDbContext _db;
    public Index(AppDbContext db) => _db = db;

    public List<UserModel> Users { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.ToLower();
            query = query.Where(u => u.Login.ToLower().Contains(s)
                                     || u.Name.ToLower().Contains(s)
                                     || u.Surname.ToLower().Contains(s)
                                     || u.Email.ToLower().Contains(s));
        }
        Users = await query.OrderBy(u => u.Login).ToListAsync();
    }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var user = await _db.Users
            .Include(u => u.Tasks)
            .Include(u => u.Projects)
                .ThenInclude(p => p.Tasks)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user != null)
        {
            // Remove tasks assigned to this user as a worker
            _db.Tasks.RemoveRange(user.Tasks);

            // Remove tasks belonging to user's projects, then the projects themselves
            foreach (var project in user.Projects)
            {
                _db.Tasks.RemoveRange(project.Tasks);
            }
            _db.Projects.RemoveRange(user.Projects);

            _db.Users.Remove(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ErrorMessage = "Не удалось удалить пользователя: существуют связанные данные.";
            }
        }
        return RedirectToPage();
    }
}



