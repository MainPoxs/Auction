using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Online_Auction.Models;
using Online_Auction.Services;
using System;
using System.Security.Claims;

namespace Online_Auction.Controllers;
public class LoginController : Controller
{
    private readonly UserService userService;
    public LoginController(UserService userService)
    {
        this.userService = userService;
    }

    [HttpGet]
    [Route("/login")]
    public IActionResult Login()
    {
        return View("~/Views/Shared/login.cshtml");
    }

    [HttpPost]
    [Route("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginUser(string loginUs,
        string password, string returnUrl = "/")
    {      
        if (string.IsNullOrWhiteSpace(loginUs) || string.IsNullOrWhiteSpace(password))
        {
            TempData["Empty"] = "Заполните логин и пароль";
            return View("~/Views/Shared/login.cshtml");
        }
        // Ищем пользователя
        var user = await userService.FindByLoginAsync(loginUs, password);

        // Если пользователь не найден — записываем попытку
        if (user is null)
        {           
            var isBlocked = await userService.RecordFailedLoginAsync(loginUs);

            if (isBlocked)
            {
                TempData["BlockUser"] = "Аккаунт заблокирован. " +                    
                    "Напишите администратору.";
            }            
            return View("~/Views/Shared/login.cshtml");
        }

        if (user.IsBanned == true)
        {
            TempData["BlockUser"] = "Ваш аккаунт заблокирован." +
                " Обратитесь к администратору";
            return View("~/Views/Shared/login.cshtml");
        }

            if (user.Role?.RoleName is null)
        {
            ModelState.AddModelError("", "Ошибка авторизации: не назначена роль");
            return View("~/Views/Shared/login.cshtml");
        }
        // Успешный вход — сбрасываем счётчик
        await userService.ResetFailedLoginAsync(user);

        // формируем claims для аутентификации
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.LoginUs),
            new Claim(ClaimTypes.Email, user.Email ?? ""),            
            new Claim("FirstName", user.FirstName),
            new Claim(ClaimTypes.Role, user.Role.RoleName)
        };

        // создаем объект ClaimsIdentity
        var claimsIdentity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        // установка аутентификационных куки
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults
            .AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        // Redirect по роли
        if (user.Role?.RoleName == "Admin")       
            return Redirect("~/adminaccount"); 
        if(user.Role?.RoleName == "User")
            return Redirect("~/usaccount");

        return Redirect("~/login");
    }


    [HttpGet]
    [Route("/logout")]
    public async Task<IActionResult> LogoutUser()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults
            .AuthenticationScheme);
        return Redirect("/");
    }
}
