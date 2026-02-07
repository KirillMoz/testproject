using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using testproject.Data;
using testproject.Models;
using testproject.ViewModels;

namespace testproject.Controllers
{
    /// <summary>
    /// Контроллер для работы с профилем пользователя
    /// Обрабатывает просмотр и редактирование профиля
    /// </summary>
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /Profile/Index/{id?}
        /// Отображает профиль пользователя
        /// Если id не указан, показывает профиль текущего пользователя
        /// </summary>
        public IActionResult Index(int? id)
        {
            // Получаем ID текущего пользователя
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                // Если пользователь не авторизован, перенаправляем на главную
                return RedirectToAction("Index", "Home");
            }

            // Если id не указан, показываем профиль текущего пользователя
            int profileId = id ?? currentUserId.Value;

            // Загружаем пользователя из базы данных
            var user = _context.Users
                .Include(u => u.Posts) // Загружаем посты пользователя
                .FirstOrDefault(u => u.Id == profileId);

            if (user == null)
                return NotFound(); // 404 если пользователь не найден

            // Считаем количество друзей
            var friendsCount = _context.Friendships
                .Count(f => f.UserId == profileId);

            // Проверяем, является ли текущий пользователь другом
            var isFriend = _context.Friendships
                .Any(f => f.UserId == currentUserId && f.FriendId == profileId);

            // Проверяем статус заявки в друзья
            var friendRequest = _context.FriendRequests
                .FirstOrDefault(fr =>
                    (fr.SenderId == currentUserId && fr.ReceiverId == profileId) ||
                    (fr.SenderId == profileId && fr.ReceiverId == currentUserId));

            // Создаем модель для представления
            var model = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                Bio = user.Bio,
                Location = user.Location,
                Website = user.Website,
                AvatarUrl = user.AvatarUrl ?? "/images/default-avatar.png",
                IsCurrentUser = user.Id == currentUserId,
                IsFriend = isFriend,
                FriendRequestStatus = friendRequest?.Status,
                FriendsCount = friendsCount,
                PostsCount = user.Posts.Count
            };

            return View(model);
        }

        /// <summary>
        /// GET: /Profile/Edit
        /// Отображает форму редактирования профиля
        /// </summary>
        [HttpGet]
        public IActionResult Edit()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Index", "Home");

            var user = _context.Users.Find(userId.Value);

            if (user == null)
                return NotFound();

            // Создаем модель для формы редактирования
            var model = new EditProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                Location = user.Location,
                Website = user.Website,
                AvatarUrl = user.AvatarUrl
            };

            return View(model);
        }

        /// <summary>
        /// POST: /Profile/Edit
        /// Обрабатывает обновление профиля
        /// </summary>
        [HttpPost]
        public IActionResult Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                if (userId == null || model.Id != userId)
                    return Forbid(); // 403 если пытается редактировать чужой профиль

                var user = _context.Users.Find(model.Id);

                if (user == null)
                    return NotFound();

                // Обновляем данные пользователя
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Bio = model.Bio;
                user.Location = model.Location;
                user.Website = model.Website;

                // Обработка загрузки аватара
                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    // В реальном проекте здесь была бы загрузка файла на сервер
                    // Для демонстрации сохраняем только имя файла
                    user.AvatarUrl = $"/uploads/avatars/{model.AvatarFile.FileName}";
                }

                _context.SaveChanges();

                // Обновляем имя в сессии
                HttpContext.Session.SetString("UserName", user.FullName);

                return RedirectToAction("Index", new { id = user.Id });
            }

            // Если есть ошибки валидации, показываем форму снова
            return View(model);
        }

        /// <summary>
        /// POST: /Profile/CreatePost
        /// Создает новый пост на стене
        /// </summary>
        [HttpPost]
        public IActionResult CreatePost(int wallOwnerId, string content)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null || string.IsNullOrEmpty(content))
                return BadRequest(); // 400 если данные неверные

            var post = new Post
            {
                AuthorId = currentUserId.Value,
                WallOwnerId = wallOwnerId,
                Content = content,
                CreatedDate = DateTime.Now,
                LikesCount = 0
            };

            _context.Posts.Add(post);
            _context.SaveChanges();

            return RedirectToAction("Index", new { id = wallOwnerId });
        }

        /// <summary>
        /// POST: /Profile/DeletePost/{id}
        /// Удаляет пост
        /// </summary>
        [HttpPost]
        public IActionResult DeletePost(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(); // 401 если не авторизован

            var post = _context.Posts.Find(id);

            if (post == null)
                return NotFound();

            // Проверяем, что пост принадлежит текущему пользователю
            if (post.AuthorId != currentUserId && post.WallOwnerId != currentUserId)
                return Forbid(); // 403 если пытается удалить чужой пост

            _context.Posts.Remove(post);
            _context.SaveChanges();

            return Ok();
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }
    }
}