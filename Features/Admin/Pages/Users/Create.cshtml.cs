using System.ComponentModel.DataAnnotations;
using ClientForge.Data;
using ClientForge.Features.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UserModel = ClientForge.Features.User.Models.User;

namespace ClientForge.Features.Admin.Pages.Users;

[Authorize(Roles = "admin")]
public class Create : PageModel
{
    private readonly AppDbContext _db;
    public Create(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(50)] public string Login { get; set; } = "";
        [Required, MaxLength(50)] public string Name { get; set; } = "";
        [Required, MaxLength(50)] public string Surname { get; set; } = "";
        [Required, MaxLength(50), EmailAddress] public string Email { get; set; } = "";
        [Required, MinLength(4)] public string Password { get; set; } = "";
        public Role Role { get; set; } = Role.guest;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (_db.Users.Any(u => u.Login == Input.Login))
        {
            ModelState.AddModelError("Input.Login", "Login already exists.");
            return Page();
        }

        var user = new UserModel
        {
            Login = Input.Login,
            Name = Input.Name,
            Surname = Input.Surname,
            Email = Input.Email,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(Input.Password),
            Role = Input.Role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return RedirectToPage("/Features/Admin/Pages/Users/Index");
    }
}


