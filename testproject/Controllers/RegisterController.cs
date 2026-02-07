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
    /// Контроллер для обработки регистрации пользователей
    /// Отдельный от HomeController для разделения ответственности
    /// </summary>
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Конструктор с Dependency Injection
        public RegisterController(ApplicationDbContext context)
        {
            _context = context; // Получаем контекст базы данных
        }

        /// <summary>
        /// GET: /Register/Index
        /// Показывает форму регистрации (альтернативный вариант)
        /// В текущей реализации используется Home/Index
        /// </summary>
        public IActionResult Index()
        {
            var model = new AccountViewModel
            {
                RegisterModel = new RegisterViewModel(),
                LoginModel = new LoginViewModel(),
                SelectLists = GetSelectLists()
            };

            return View(model);
        }

        /// <summary>
        /// POST: /Register/Register
        /// Основной метод регистрации нового пользователя
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        public IActionResult Register(RegisterViewModel model)
        {
            // Проверяем валидность данных из формы
            if (ModelState.IsValid)
            {
                // Проверяем, что все компоненты даты рождения выбраны
                if (!model.BirthYear.HasValue || !model.BirthMonth.HasValue || !model.BirthDay.HasValue)
                {
                    ModelState.AddModelError("", "Выберите полную дату рождения");
                    return ReturnToFormWithError(model);
                }

                try
                {
                    // Создаем DateTime из отдельных компонентов
                    var birthDate = new DateTime(model.BirthYear.Value, model.BirthMonth.Value, model.BirthDay.Value);

                    // Проверяем возраст пользователя (должен быть 18+)
                    var age = CalculateAge(birthDate);
                    if (age < 18)
                    {
                        ModelState.AddModelError("", "Вам должно быть не менее 18 лет");
                        return ReturnToFormWithError(model);
                    }

                    // Проверяем, не зарегистрирован ли уже такой email
                    if (IsEmailAlreadyRegistered(model.Email))
                    {
                        ModelState.AddModelError("Email", "Этот email уже зарегистрирован");
                        return ReturnToFormWithError(model);
                    }

                    // Создаем нового пользователя
                    var user = CreateUserFromModel(model, birthDate);

                    // Сохраняем в базу данных
                    SaveUserToDatabase(user);

                    // Автоматически входим после успешной регистрации
                    LoginUserAfterRegistration(user);

                    // Перенаправляем на страницу профиля
                    return RedirectToAction("Index", "Profile");
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    // Обрабатываем некорректную дату (например, 30 февраля)
                    ModelState.AddModelError("", $"Некорректная дата рождения: {ex.Message}");
                    return ReturnToFormWithError(model);
                }
                catch (Exception ex)
                {
                    // Общая обработка ошибок
                    ModelState.AddModelError("", $"Ошибка при регистрации: {ex.Message}");
                    return ReturnToFormWithError(model);
                }
            }

            // Если данные не валидны, возвращаем форму с ошибками
            return ReturnToFormWithError(model);
        }

        // ==================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ====================

        /// <summary>
        /// Создает пользователя из модели регистрации
        /// </summary>
        private User CreateUserFromModel(RegisterViewModel model, DateTime birthDate)
        {
            return new User
            {
                FirstName = model.FirstName?.Trim() ?? "",
                LastName = model.LastName?.Trim() ?? "",
                Email = model.Email?.Trim()?.ToLower() ?? "", // Приводим email к нижнему регистру
                BirthDate = birthDate,
                RegistrationDate = DateTime.Now,
                PasswordHash = HashPassword(model.Password ?? ""), // Хэшируем пароль
                Bio = "",
                Location = "",
                Website = "",
                AvatarUrl = "/images/default-avatar.png" // Аватар по умолчанию
            };
        }

        /// <summary>
        /// Сохраняет пользователя в базу данных
        /// </summary>
        private void SaveUserToDatabase(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges(); // Сохраняем изменения в БД
        }

        /// <summary>
        /// Выполняет автоматический вход после регистрации
        /// </summary>
        private void LoginUserAfterRegistration(User user)
        {
            // Сохраняем ID пользователя в сессии для аутентификации
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
        }

        /// <summary>
        /// Вычисляет возраст пользователя по дате рождения
        /// </summary>
        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            // Корректируем возраст, если день рождения еще не наступил в этом году
            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }

        /// <summary>
        /// Проверяет, зарегистрирован ли уже email
        /// </summary>
        private bool IsEmailAlreadyRegistered(string email)
        {
            return _context.Users.Any(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Хэширует пароль с использованием SHA256
        /// ВНИМАНИЕ: В реальных проектах используйте BCrypt, Argon2 или ASP.NET Core Identity!
        /// </summary>
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                // Преобразуем пароль в байты и вычисляем хэш
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                // Конвертируем байты в base64 строку для хранения
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Возвращает на форму с ошибками валидации
        /// </summary>
        private IActionResult ReturnToFormWithError(RegisterViewModel model)
        {
            var viewModel = new AccountViewModel
            {
                LoginModel = new LoginViewModel(),
                RegisterModel = model,
                SelectLists = GetSelectLists()
            };

            return View("~/Views/Home/Index.cshtml", viewModel);
        }

        /// <summary>
        /// Генерирует списки для выбора даты рождения
        /// </summary>
        private SelectListsViewModel GetSelectLists()
        {
            return new SelectListsViewModel
            {
                Days = GenerateDaysList(),
                Months = GenerateMonthsList(),
                Years = GenerateYearsList()
            };
        }

        /// <summary>
        /// Генерирует список годов (текущий год - 100 лет)
        /// </summary>
        private List<SelectListItem> GenerateYearsList()
        {
            var years = new List<SelectListItem>();
            var currentYear = DateTime.Now.Year;

            // Добавляем placeholder
            years.Add(new SelectListItem
            {
                Value = "",
                Text = "Год",
                Selected = true,
                Disabled = true
            });

            // Генерируем 100 последних лет
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

        /// <summary>
        /// Генерирует список месяцев
        /// </summary>
        private List<SelectListItem> GenerateMonthsList()
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

        /// <summary>
        /// Генерирует список дней (1-31)
        /// </summary>
        private List<SelectListItem> GenerateDaysList()
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

        /// <summary>
        /// Проверяет валидность даты рождения
        /// </summary>
        private bool IsValidBirthDate(int? day, int? month, int? year)
        {
            if (!day.HasValue || !month.HasValue || !year.HasValue)
                return false;

            try
            {
                var date = new DateTime(year.Value, month.Value, day.Value);
                return date <= DateTime.Now; // Дата рождения не может быть в будущем
            }
            catch
            {
                return false; // Некорректная дата
            }
        }

        /// <summary>
        /// GET: /Register/CheckEmailAvailability
        /// Проверяет доступность email (для AJAX запросов)
        /// </summary>
        [HttpGet]
        public IActionResult CheckEmailAvailability(string email)
        {
            if (string.IsNullOrEmpty(email))
                return Json(new { available = false, message = "Email не может быть пустым" });

            var isAvailable = !_context.Users.Any(u => u.Email.ToLower() == email.ToLower());

            return Json(new
            {
                available = isAvailable,
                message = isAvailable ? "Email доступен" : "Email уже занят"
            });
        }

        /// <summary>
        /// GET: /Register/Success
        /// Страница успешной регистрации (если нужна отдельная)
        /// </summary>
        public IActionResult Success()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Home");

            return View();
        }
    }
}