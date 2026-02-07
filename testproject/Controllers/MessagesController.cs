using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using testproject.Data;
using testproject.Models;
using testproject.ViewModels;

namespace testproject.Controllers
{
    /// <summary>
    /// Контроллер для работы с личными сообщениями
    /// Обрабатывает отправку, получение и отображение сообщений
    /// </summary>
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /Messages/Index
        /// Показывает список диалогов текущего пользователя
        /// </summary>
        public IActionResult Index()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return RedirectToAction("Index", "Home");

            // Получаем последние диалоги
            var dialogs = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Select(g => new DialogViewModel
                {
                    OtherUserId = g.Key,
                    OtherUserName = g.Key == g.First().SenderId ?
                        g.First().Sender.FullName :
                        g.First().Receiver.FullName,
                    OtherUserAvatar = g.Key == g.First().SenderId ?
                        g.First().Sender.AvatarUrl ?? "/images/default-avatar.png" :
                        g.First().Receiver.AvatarUrl ?? "/images/default-avatar.png",
                    LastMessage = g.OrderByDescending(m => m.SentDate).First().Content,
                    LastMessageDate = g.OrderByDescending(m => m.SentDate).First().SentDate,
                    UnreadCount = g.Count(m => !m.IsRead && m.ReceiverId == currentUserId)
                })
                .OrderByDescending(d => d.LastMessageDate) // Сортируем по дате последнего сообщения
                .ToList();

            return View(dialogs);
        }

        /// <summary>
        /// GET: /Messages/Chat/{userId}
        /// Показывает диалог с конкретным пользователем
        /// </summary>
        public IActionResult Chat(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return RedirectToAction("Index", "Home");

            // Проверяем, существует ли пользователь
            var otherUser = _context.Users.Find(userId);
            if (otherUser == null)
                return NotFound();

            // Проверяем, являются ли пользователи друзьями
            var areFriends = _context.Friendships
                .Any(f => f.UserId == currentUserId && f.FriendId == userId);

            if (!areFriends)
            {
                TempData["ErrorMessage"] = "Вы можете отправлять сообщения только друзьям";
                return RedirectToAction("Index");
            }

            // Получаем все сообщения между пользователями
            var messages = _context.Messages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                           (m.SenderId == userId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentDate) // Сортируем по дате отправки (от старых к новым)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SenderName = m.Sender.FullName,
                    SenderAvatar = m.Sender.AvatarUrl ?? "/images/default-avatar.png",
                    Content = m.Content,
                    SentDate = m.SentDate,
                    IsRead = m.IsRead,
                    IsCurrentUserSender = m.SenderId == currentUserId
                })
                .ToList();

            // Помечаем сообщения как прочитанные
            var unreadMessages = _context.Messages
                .Where(m => m.SenderId == userId &&
                           m.ReceiverId == currentUserId &&
                           !m.IsRead);

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }
            _context.SaveChanges();

            // Создаем модель для чата
            var model = new ChatViewModel
            {
                OtherUserId = userId,
                OtherUserName = otherUser.FullName,
                OtherUserAvatar = otherUser.AvatarUrl ?? "/images/default-avatar.png",
                Messages = messages,
                NewMessage = ""
            };

            return View(model);
        }

        /// <summary>
        /// POST: /Messages/SendMessage
        /// Отправляет новое сообщение
        /// </summary>
        [HttpPost]
        public IActionResult SendMessage(ChatViewModel model)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            if (string.IsNullOrEmpty(model.NewMessage?.Trim()))
            {
                TempData["ErrorMessage"] = "Сообщение не может быть пустым";
                return RedirectToAction("Chat", new { userId = model.OtherUserId });
            }

            // Создаем новое сообщение
            var message = new Message
            {
                SenderId = currentUserId.Value,
                ReceiverId = model.OtherUserId,
                Content = model.NewMessage.Trim(),
                SentDate = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);
            _context.SaveChanges();

            return RedirectToAction("Chat", new { userId = model.OtherUserId });
        }

        /// <summary>
        /// POST: /Messages/DeleteMessage/{id}
        /// Удаляет сообщение
        /// </summary>
        [HttpPost]
        public IActionResult DeleteMessage(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            var message = _context.Messages.Find(id);

            if (message == null)
                return NotFound();

            // Проверяем, что сообщение принадлежит текущему пользователю
            if (message.SenderId != currentUserId)
                return Forbid();

            _context.Messages.Remove(message);
            _context.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// GET: /Messages/GetUnreadCount
        /// Возвращает количество непрочитанных сообщений (для AJAX запросов)
        /// </summary>
        public IActionResult GetUnreadCount()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Json(0);

            var unreadCount = _context.Messages
                .Count(m => m.ReceiverId == currentUserId && !m.IsRead);

            return Json(unreadCount);
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }
    }
}