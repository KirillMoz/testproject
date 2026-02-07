using System;
using System.ComponentModel.DataAnnotations;

namespace testproject.Models
{
    /// <summary>
    /// Модель личного сообщения между пользователями
    /// </summary>
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; } // ID отправителя
        public int ReceiverId { get; set; } // ID получателя

        [Required]
        [StringLength(2000)] // Ограничение длины сообщения
        public string Content { get; set; } = string.Empty; // Текст сообщения

        public DateTime SentDate { get; set; } // Дата и время отправки
        public bool IsRead { get; set; } = false; // Флаг прочтения сообщения

        // Навигационные свойства
        public virtual User Sender { get; set; } // Отправитель
        public virtual User Receiver { get; set; } // Получатель
    }
}