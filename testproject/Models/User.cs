using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace testproject.Models
{
    /// <summary>
    /// Модель пользователя социальной сети
    /// Представляет зарегистрированного пользователя со всей его информацией
    /// </summary>
    public class User
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        [StringLength(50)] 
        public string FirstName { get; set; } = string.Empty; 

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty; 

        [Required]
        [EmailAddress] 
        [StringLength(100)]
        public string Email { get; set; } = string.Empty; 

        [Required]
        public DateTime BirthDate { get; set; } 

        [Required]
        public DateTime RegistrationDate { get; set; } 

        [Required]
        public string PasswordHash { get; set; } = string.Empty; 

        // Опциональные поля профиля
        public string? AvatarUrl { get; set; } 
        public string? Bio { get; set; } 
        public string? Location { get; set; } // Местоположение
        public string? Website { get; set; } // Веб-сайт

        // Навигационные свойства для Entity Framework (связи между таблицами)
        public virtual ICollection<FriendRequest> SentFriendRequests { get; set; } // Отправленные заявки в друзья
        public virtual ICollection<FriendRequest> ReceivedFriendRequests { get; set; } // Полученные заявки
        public virtual ICollection<Friendship> Friendships { get; set; } // Дружеские связи
        public virtual ICollection<Message> SentMessages { get; set; } // Отправленные сообщения
        public virtual ICollection<Message> ReceivedMessages { get; set; } // Полученные сообщения
        public virtual ICollection<Post> Posts { get; set; } // Посты пользователя

        // Конструктор инициализирует коллекции
        public User()
        {
            SentFriendRequests = new HashSet<FriendRequest>();
            ReceivedFriendRequests = new HashSet<FriendRequest>();
            Friendships = new HashSet<Friendship>();
            SentMessages = new HashSet<Message>();
            ReceivedMessages = new HashSet<Message>();
            Posts = new HashSet<Post>();
        }

        // Свойство только для чтения для получения полного имени
        public string FullName => $"{FirstName} {LastName}";
    }
}