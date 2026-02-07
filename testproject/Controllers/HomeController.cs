using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using testproject.Data;
using testproject.Models;
using testproject.ViewModels;

namespace testproject.Controllers
{
    /// <summary>
    /// Контроллер главной страницы (только для входа)
    /// Регистрация вынесена в отдельный RegisterController
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /Home/Index
        /// Показывает главную страницу с формой входа
        /// </summary>
        public IActionResult Index()
        {
            // Если пользователь уже авторизован, перенаправляем на профиль
            if (IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Profile");
            }

            var model = new AccountViewModel
            {
                LoginModel = new LoginViewModel(),
                RegisterModel = new RegisterViewModel(),
                SelectLists = GetSelectLists() // Для формы регистрации
            };

            return View(model);
        }

        /// <summary>
        /// POST: /Home/Login
        /// Обрабатывает вход пользователя
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    // Успешный вход
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.FullName);

                    return RedirectToAction("Index", "Profile");
                }

                ModelState.AddModelError("", "Неверный email или пароль");
            }

            // Возвращаем на главную с ошибкой
            var viewModel = new AccountViewModel
            {
                LoginModel = model,
                RegisterModel = new RegisterViewModel(),
                SelectLists = GetSelectLists()
            };

            return View("Index", viewModel);
        }

        /// <summary>
        /// GET: /Home/Logout
        /// Выход из системы
        /// </summary>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // ==================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ====================

        private bool IsUserAuthenticated()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private SelectListsViewModel GetSelectLists()
        {
            return new SelectListsViewModel
            {
                Days = GetDays(),
                Months = GetMonths(),
                Years = GetYears()
            };
        }

        private List<SelectListItem> GetYears()
        {
            var years = new List<SelectListItem>();
            var currentYear = DateTime.Now.Year;

            years.Add(new SelectListItem
            {
                Value = "",
                Text = "Год",
                Selected = true,
                Disabled = true
            });

            for (int year = currentYear; year >= currentYear - 100; year--)
            {
                years.Add(new SelectListItem
                {
                    Value = year.ToString(),
                    Text = year.ToString()
                });
            }

            return years;
        }

        private List<SelectListItem> GetMonths()
        {
            var months = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "Месяц",
                    Selected = true,
                    Disabled = true
                }
            };

            var monthNames = new[]
            {
                "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
            };

            for (int i = 0; i < monthNames.Length; i++)
            {
                months.Add(new SelectListItem
                {
                    Value = (i + 1).ToString(),
                    Text = monthNames[i]
                });
            }

            return months;
        }

        private List<SelectListItem> GetDays()
        {
            var days = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "День",
                    Selected = true,
                    Disabled = true
                }
            };

            for (int day = 1; day <= 31; day++)
            {
                days.Add(new SelectListItem
                {
                    Value = day.ToString(),
                    Text = day.ToString()
                });
            }

            return days;
        }
    }
}