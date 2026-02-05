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
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            var model = new AccountViewModel
            {
                RegisterModel = new RegisterViewModel(),
                LoginModel = new LoginViewModel(),
                SelectLists = GetSelectLists()
            };

            return View("~/Views/Home/Index.cshtml", model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Проверка даты рождения
                if (!model.BirthYear.HasValue || !model.BirthMonth.HasValue || !model.BirthDay.HasValue)
                {
                    ModelState.AddModelError("", "Выберите полную дату рождения");
                }
                else
                {
                    try
                    {
                        var birthDate = new DateTime(model.BirthYear.Value, model.BirthMonth.Value, model.BirthDay.Value);

                        // Проверка возраста (например, 18+)
                        var age = DateTime.Now.Year - birthDate.Year;
                        if (DateTime.Now < birthDate.AddYears(age)) age--;

                        if (age < 18)
                        {
                            ModelState.AddModelError("", "Вам должно быть не менее 18 лет");
                        }
                        else
                        {
                            // Проверка email на уникальность
                            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                            if (existingUser != null)
                            {
                                ModelState.AddModelError("Email", "Этот email уже зарегистрирован");
                            }
                            else
                            {
                                // Создание нового пользователя
                                var user = new User
                                {
                                    FirstName = model.FirstName ?? string.Empty,
                                    LastName = model.LastName ?? string.Empty,
                                    Email = model.Email ?? string.Empty,
                                    BirthDate = birthDate,
                                    RegistrationDate = DateTime.Now,
                                    PasswordHash = HashPassword(model.Password ?? string.Empty),
                                    AvatarUrl = string.Empty,
                                    Bio = string.Empty,
                                    Location = string.Empty
                                };

                                _context.Users.Add(user);
                                _context.SaveChanges();

                                TempData["SuccessMessage"] = $"Регистрация прошла успешно, {model.FirstName}! Теперь вы можете войти.";
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        ModelState.AddModelError("", "Некорректная дата рождения");
                    }
                }
            }

            // Если есть ошибки, возвращаем на страницу с формой
            var viewModel = new AccountViewModel
            {
                RegisterModel = model,
                LoginModel = new LoginViewModel(),
                SelectLists = GetSelectLists()
            };

            return View("~/Views/Home/Index.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Поиск пользователя по email
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    // Проверка пароля
                    if (VerifyPassword(model.Password ?? string.Empty, user.PasswordHash))
                    {
                        // Успешный вход
                        TempData["SuccessMessage"] = $"Добро пожаловать, {user.FirstName}!";
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Неверный email или пароль");
            }

            // Если есть ошибки, возвращаем на страницу с формой
            var viewModel = new AccountViewModel
            {
                RegisterModel = new RegisterViewModel(),
                LoginModel = model,
                SelectLists = GetSelectLists()
            };

            return View("~/Views/Home/Index.cshtml", viewModel);
        }

        // Метод для хэширования пароля (SHA256)
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

        // Метод для проверки пароля
        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
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