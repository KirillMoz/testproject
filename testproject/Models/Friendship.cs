using System;

namespace testproject.Models
{
    /// <summary>
    /// Модель дружеской связи
    /// Хранит информацию о том, что два пользователя являются друзьями
    /// Важно: дружба двусторонняя, создаются две записи (User→Friend и Friend→User)
    /// </summary>
    public class Friendship
    {
        public int Id { get; set; }

        public int UserId { get; set; } // ID первого пользователя
        public int FriendId { get; set; } // ID второго пользователя (друга)

        public DateTime BecameFriendsDate { get; set; } // Дата установления дружбы

        // Навигационные свойства
        public virtual User User { get; set; } // Пользователь
        public virtual User Friend { get; set; } // Его друг
    }
}