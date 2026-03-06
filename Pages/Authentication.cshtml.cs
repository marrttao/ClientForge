using System.ComponentModel.DataAnnotations;
using System.Security.Claims;          // Claim, ClaimTypes
using System.Text;                     // Encoding
using ClientForge.Features.User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;  // SymmetricSecurityKey, SigningCredentials
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt; // JwtSecurityToken, JwtSecurityTokenHandler
using ClientForge.Data;
namespace ClientForge.Pages;

public class Authentication : PageModel
{
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, "Admin") // Roles go here too
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_Very_Long_And_Secret_Key_123!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "MyApi",
            audience: "MyFrontend",
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private readonly AppDbContext _dbContext;
    private readonly ILogger<Authentication> _logger;

    public Authentication(AppDbContext dbContext, ILogger<Authentication> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // User model for login: bind so that asp-for="User.Login" fills this property
    [BindProperty]
    public new User User { get; set; } = new();

    // Password for login: also bound
    [BindProperty]
    [Required(ErrorMessage = "Please enter a password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    
    // POST handler for login (form with asp-page-handler="Login")
    public IActionResult OnPostLogin()
    {
        // Clear all validation errors (User model and Register model have [Required] fields
        // that are irrelevant for the login form)
        ModelState.Clear();

        _logger.LogInformation("OnPostLogin called with User.Login={Login}", User.Login);

        if (string.IsNullOrWhiteSpace(User.Login) || string.IsNullOrWhiteSpace(Password))
        {
            ModelState.AddModelError(string.Empty, "Please enter login and password.");
            return Page();
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Login == User.Login);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login or password.");
            return Page();
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(Password, user.HashedPassword);

        if (!isPasswordValid)
        {
            ModelState.AddModelError(string.Empty, "Invalid login or password.");
            return Page();
        }

        var token = GenerateToken(user);

        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(3)
        });

        _logger.LogInformation("User logged in: {Login}", user.Login);

        return RedirectToPage("/Index");
    }

    public class RegisterInput
    {
        [Required]
        [MaxLength(50)]
        [MinLength(1)]
        public string Login { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [MinLength(1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [MinLength(1)]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [MinLength(1)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MaxLength(100)]
        [MinLength(1)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    [BindProperty]
    public RegisterInput Register { get; set; } = new();

    public void OnGet()
    {
        // Page initialization on GET request (add logic if needed)
    }

    // POST handler for registration (form with asp-page-handler="Register")
    public IActionResult OnPostRegister()
    {
        // Remove validation errors for login-form fields that are irrelevant here
        var keysToRemove = ModelState.Keys
            .Where(k => !k.StartsWith("Register", StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var key in keysToRemove)
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            // Temporary verbose logging of all ModelState errors to see what went wrong
            foreach (var kvp in ModelState)
            {
                var key = kvp.Key;
                var errors = kvp.Value.Errors;
                foreach (var error in errors)
                {
                    _logger.LogWarning("ModelState error on key '{Key}': {ErrorMessage}", key, error.ErrorMessage);
                }
            }

            _logger.LogWarning("Registration failed: ModelState is invalid");
            return Page();
        }

        try
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Register.Password);

            var user = new User
            {
                Login = Register.Login,
                Name = Register.Name,
                Surname = Register.Surname,
                Email = Register.Email,
                HashedPassword = hashedPassword,
                Role = Role.guest
            };
            if (_dbContext.Users.Any(u => u.Login == user.Login))
            {
                ModelState.AddModelError(string.Empty, "Login already exists. Please choose a different login.");
                return Page();
            }
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            _logger.LogInformation("New user registered: {Login} ({Email})", user.Login, user.Email);

            // Redirect after successful registration to prevent form resubmission
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while registering user {Login}", Register.Login);
            ModelState.AddModelError(string.Empty, "Failed to save user. Please try again later.");
            return Page();
        }
    }
}

