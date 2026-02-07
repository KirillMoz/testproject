using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using testproject.Data;
using testproject.Models;
using testproject.ViewModels;

namespace testproject.Controllers
{
    /// <summary>
    /// Контроллер для работы с друзьями
    /// Обрабатывает поиск, добавление, удаление друзей
    /// </summary>
    public class FriendsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FriendsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /Friends/Index
        /// Показывает список друзей текущего пользователя
        /// </summary>
        public IActionResult Index()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return RedirectToAction("Index", "Home");

            // Загружаем друзей текущего пользователя
            var friends = _context.Friendships
                .Include(f => f.Friend) // Загружаем информацию о друге
                .Where(f => f.UserId == currentUserId.Value)
                .Select(f => new FriendViewModel
                {
                    Id = f.Friend.Id,
                    FullName = f.Friend.FirstName + " " + f.Friend.LastName,
                    AvatarUrl = f.Friend.AvatarUrl ?? "/images/default-avatar.png",
                    Location = f.Friend.Location
                })
                .ToList();

            // Загружаем входящие заявки в друзья
            var incomingRequests = _context.FriendRequests
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == currentUserId && fr.Status == FriendRequestStatus.Pending)
                .Select(fr => new FriendRequestViewModel
                {
                    Id = fr.Id,
                    SenderId = fr.Sender.Id,
                    SenderName = fr.Sender.FirstName + " " + fr.Sender.LastName,
                    SenderAvatar = fr.Sender.AvatarUrl ?? "/images/default-avatar.png",
                    SentDate = fr.SentDate
                })
                .ToList();

            ViewBag.IncomingRequests = incomingRequests;

            return View(friends);
        }

        /// <summary>
        /// GET: /Friends/Search
        /// Показывает результаты поиска пользователей
        /// </summary>
        public IActionResult Search(string query)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return RedirectToAction("Index", "Home");

            var model = new SearchResultsViewModel();

            if (!string.IsNullOrEmpty(query))
            {
                // Ищем пользователей по имени, фамилии или email
                var users = _context.Users
                    .Where(u => u.Id != currentUserId.Value &&
                               (u.FirstName.Contains(query) ||
                                u.LastName.Contains(query) ||
                                u.Email.Contains(query)))
                    .Select(u => new FriendViewModel
                    {
                        Id = u.Id,
                        FullName = u.FirstName + " " + u.LastName,
                        AvatarUrl = u.AvatarUrl ?? "/images/default-avatar.png",
                        Location = u.Location
                    })
                    .ToList();

                model.Query = query;
                model.Users = users;
            }

            return View(model);
        }

        /// <summary>
        /// POST: /Friends/SendFriendRequest
        /// Отправляет заявку в друзья
        /// </summary>
        [HttpPost]
        public IActionResult SendFriendRequest(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            // Проверяем, не отправили ли уже заявку
            var existingRequest = _context.FriendRequests
                .FirstOrDefault(fr => fr.SenderId == currentUserId &&
                                     fr.ReceiverId == userId);

            if (existingRequest == null)
            {
                // Проверяем, не являются ли уже друзьями
                var areAlreadyFriends = _context.Friendships
                    .Any(f => f.UserId == currentUserId && f.FriendId == userId);

                if (!areAlreadyFriends)
                {
                    var request = new FriendRequest
                    {
                        SenderId = currentUserId.Value,
                        ReceiverId = userId,
                        SentDate = DateTime.Now,
                        Status = FriendRequestStatus.Pending
                    };

                    _context.FriendRequests.Add(request);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Заявка в друзья отправлена!";
                }
            }

            return RedirectToAction("Search", new { query = "" });
        }

        /// <summary>
        /// POST: /Friends/AcceptFriendRequest
        /// Принимает заявку в друзья
        /// </summary>
        [HttpPost]
        public IActionResult AcceptFriendRequest(int requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            var request = _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .FirstOrDefault(fr => fr.Id == requestId);

            if (request == null || request.ReceiverId != currentUserId)
                return NotFound();

            // Обновляем статус заявки
            request.Status = FriendRequestStatus.Accepted;

            // Создаем двустороннюю дружбу
            var friendship1 = new Friendship
            {
                UserId = request.SenderId,
                FriendId = request.ReceiverId,
                BecameFriendsDate = DateTime.Now
            };

            var friendship2 = new Friendship
            {
                UserId = request.ReceiverId,
                FriendId = request.SenderId,
                BecameFriendsDate = DateTime.Now
            };

            _context.Friendships.Add(friendship1);
            _context.Friendships.Add(friendship2);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Вы приняли заявку в друзья!";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /Friends/RejectFriendRequest
        /// Отклоняет заявку в друзья
        /// </summary>
        [HttpPost]
        public IActionResult RejectFriendRequest(int requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            var request = _context.FriendRequests.Find(requestId);

            if (request == null || request.ReceiverId != currentUserId)
                return NotFound();

            request.Status = FriendRequestStatus.Rejected;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /Friends/RemoveFriend
        /// Удаляет пользователя из друзей
        /// </summary>
        [HttpPost]
        public IActionResult RemoveFriend(int friendId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            // Удаляем обе записи о дружбе (User→Friend и Friend→User)
            var friendships = _context.Friendships
                .Where(f => (f.UserId == currentUserId && f.FriendId == friendId) ||
                           (f.UserId == friendId && f.FriendId == currentUserId))
                .ToList();

            _context.Friendships.RemoveRange(friendships);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Пользователь удален из друзей";

            return RedirectToAction("Index");
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }
    }
}