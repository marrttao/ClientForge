using System.ComponentModel.DataAnnotations;
using ClientForge.Features.User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BCrypt.Net;
namespace ClientForge.Pages;

public class Authentication : PageModel
{
    // Модель пользователя для регистрации / логина
    [BindProperty]
    public new User User { get; set; } = new();

    // Пароль для логина
    [BindProperty]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // Пароль и подтверждение для регистрации
    [BindProperty]
    [DataType(DataType.Password)]
    public string RegisterPassword { get; set; } = string.Empty;

    [BindProperty]
    [DataType(DataType.Password)]
    [Compare(nameof(RegisterPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;

    public void OnGet()
    {
        // Инициализация страницы при GET-запросе (при необходимости добавь логику сам)
    }

    // Обработчик POST для логина (форма с asp-page-handler="Login")
    public IActionResult OnPostLogin()
    {
        // TODO: реализуй свою логику аутентификации
        // - найти пользователя по User.Login
        // - проверить введённый Password
        // - установить куки/сессию и т.п.

        return Page();
    }

    // Обработчик POST для регистрации (форма с asp-page-handler="Register")
    public IActionResult OnPostRegister()
    {
        // TODO: реализуй свою логику регистрации
        // - проверить ModelState
        // - проверить совпадение RegisterPassword и ConfirmPassword (есть атрибут Compare)
        // - захэшировать пароль и сохранить пользователя
        
        return Page();
    }
}