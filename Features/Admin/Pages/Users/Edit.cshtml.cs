using System.ComponentModel.DataAnnotations;
using ClientForge.Data;
using ClientForge.Features.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClientForge.Features.Admin.Pages.Users;

[Authorize(Roles = "admin")]
public class Edit : PageModel
{
    private readonly AppDbContext _db;
    public Edit(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public Guid Id { get; set; }

    public class InputModel
    {
        [Required, MaxLength(50)] public string Login { get; set; } = "";
        [Required, MaxLength(50)] public string Name { get; set; } = "";
        [Required, MaxLength(50)] public string Surname { get; set; } = "";
        [Required, MaxLength(50), EmailAddress] public string Email { get; set; } = "";
        public string? Password { get; set; }
        public Role Role { get; set; } = Role.guest;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return RedirectToPage("/Features/Admin/Pages/Users/Index");

        Id = id;
        Input = new InputModel
        {
            Login = user.Login,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Role = user.Role
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _db.Users.FindAsync(Id);
        if (user == null) return RedirectToPage("/Features/Admin/Pages/Users/Index");

        if (_db.Users.Any(u => u.Login == Input.Login && u.Id != Id))
        {
            ModelState.AddModelError("Input.Login", "Login already exists.");
            return Page();
        }

        user.Login = Input.Login;
        user.Name = Input.Name;
        user.Surname = Input.Surname;
        user.Email = Input.Email;
        user.Role = Input.Role;

        if (!string.IsNullOrWhiteSpace(Input.Password))
        {
            user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(Input.Password);
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("/Features/Admin/Pages/Users/Index");
    }
}

