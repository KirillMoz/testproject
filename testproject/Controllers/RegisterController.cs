using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using testproject.Models;
using testproject.ViewModels;

namespace testproject.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Устанавливаем текущий год по умолчанию
            var model = new RegisterViewModel
            {
                Year = DateTime.Now.Year - 18, // 18 лет по умолчанию
                Month = DateTime.Now.Month,
                Date = DateTime.Now.Day
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Сохраняем введенные значения для повторного отображения
                return View(model);
            }

            try
            {
                // Проверка email
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(model.Email))
                {
                    ModelState.AddModelError("Email", "Введите корректный email адрес (например: example@mail.com)");
                    return View(model);
                }

                // Проверка возраста
                var age = DateTime.Now.Year - model.Year;
                if (age < 13)
                {
                    ModelState.AddModelError("", "Для регистрации необходимо быть старше 13 лет");
                    return View(model);
                }

                // Проверка никнейма
                var existingUser = await _userManager.FindByNameAsync(model.Login);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Login", "Этот никнейм уже занят");
                    return View(model);
                }

                // Проверка email
                existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Этот email уже зарегистрирован");
                    return View(model);
                }

                // Создаем пользователя
                var user = new User
                {
                    UserName = model.Login,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = new DateTime(model.Year, model.Month, model.Date)
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка при регистрации: {ex.Message}");
            }

            return View(model);
        }

        // AJAX проверка никнейма
        [HttpGet]
        public async Task<JsonResult> CheckLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login) || login.Length < 3)
            {
                return Json(new { available = false, message = "Никнейм должен быть не менее 3 символов" });
            }

            var user = await _userManager.FindByNameAsync(login);
            return Json(new { available = user == null, message = user == null ? "Никнейм доступен" : "Никнейм уже занят" });
        }

        // AJAX проверка email
        [HttpGet]
        public async Task<JsonResult> CheckEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { available = false, message = "Введите email" });
            }

            var user = await _userManager.FindByEmailAsync(email);
            return Json(new { available = user == null, message = user == null ? "Email доступен" : "Email уже зарегистрирован" });
        }
    }
}