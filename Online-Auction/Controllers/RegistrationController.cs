using Microsoft.AspNetCore.Mvc;
using Online_Auction.Models;
using Online_Auction.Services;

namespace Online_Auction.Controllers;
public class RegistrationController : Controller
{
    private readonly UserService userService;
    public RegistrationController(UserService userService)
    {
        this.userService = userService;
    }

    [HttpGet]
    [Route("/registration")]
    public IActionResult Registration()
    {
        return View();
    }

    [HttpPost]
    [Route("/registration")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterUser(
        string firstName,
        string lastName,
        string email,
        string loginUs,
        string password)
    {
        // Валидация входных данных
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(loginUs) ||
            string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Все поля обязательны");
        }

        // Проверка: не занят ли логин и email
        if (await userService.IsLoginEmailTakenAsync(loginUs, email))
        {
            return BadRequest("Пользователь с такими данными" +
                " уже существует");
        }

        User user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            LoginUs = loginUs
        };

        bool result = await userService.AddUserAsync(user, password);

        if (!result)
        {
            return BadRequest("Не удалось создать пользователя." +
                " Попробуйте позже.");
        }
        return RedirectToAction("Login", "Login");
    }
}
