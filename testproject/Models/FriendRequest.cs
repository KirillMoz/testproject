using System;

namespace testproject.Models
{
    /// <summary>
    /// Модель заявки в друзья
    /// Связывает отправителя и получателя заявки
    /// </summary>
    public class FriendRequest
    {
        public int Id { get; set; } // Уникальный идентификатор заявки

        // Внешние ключи для связи с таблицей Users
        public int SenderId { get; set; } // ID отправителя
        public int ReceiverId { get; set; } // ID получателя

        public DateTime SentDate { get; set; } // Дата отправки заявки

        // Статус заявки: Ожидание, Принята, Отклонена, Заблокирована
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

        // Навигационные свойства для связи с User
        public virtual User Sender { get; set; } // Отправитель заявки
        public virtual User Receiver { get; set; } // Получатель заявки
    }

    /// <summary>
    /// Перечисление статусов заявки в друзья
    /// </summary>
    public enum FriendRequestStatus
    {
        Pending,    // Ожидает решения получателя
        Accepted,   // Принята - пользователи стали друзьями
        Rejected,   // Отклонена получателем
        Blocked     // Заблокирована (отправка новых заявок запрещена)
    }
}